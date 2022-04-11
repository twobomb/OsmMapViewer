<?PHP
require_once "./db_config.php";


function getBBoxFromGeoJson($json,$cc = 0.0001){//$cc минимальная ширина\высота рамки, расширит если меньше
	$g = json_decode($json,true);
	$arr = [];
	
	if($g["type"] == "Polygon"){
		//((MINX, MINY), (MINX, MAXY), (MAXX, MAXY), (MAXX, MINY), (MINX, MINY))
		//to 'minlat','maxlat','minlon','maxlon'
		$arr = [ 
			$g["coordinates"][0][0][1],
			$g["coordinates"][0][1][1],
			$g["coordinates"][0][1][0],
			$g["coordinates"][0][2][0]
		];
	}
	if($g["type"] == "Point"){
		//((MINX, MINY), (MINX, MAXY), (MAXX, MAXY), (MAXX, MINY), (MINX, MINY))
		//to 'minlat','maxlat','minlon','maxlon'
		$arr = [ 
			$g["coordinates"][1],
			$g["coordinates"][1],
			$g["coordinates"][0],
			$g["coordinates"][0]
		];
	}	
	//расширение
	if($arr[1] - $arr[0] < $cc){
		$cen = $arr[0] + ($arr[1] - $arr[0])/2;
		$arr[0] = $cen - $cc/2;
		$arr[1] = $cen + $cc/2;
	}
	if($arr[3] - $arr[2] < $cc){
		$cen = $arr[2] + ($arr[3] - $arr[2])/2;
		$arr[2] = $cen - $cc/2;
		$arr[3] = $cen + $cc/2;
	}
	return $arr;
}

if($_SERVER["REQUEST_METHOD"] == "POST"):
	$tags = json_decode(file_get_contents('php://input'));
	

$dbconn = pg_connect("host=$DB_HOST dbname=$DB_NAME user=$DB_USER password=$DB_PWD");
if($dbconn === false){
	http_response_code(505);
	die;
}

$tables =[];

try{

if(!isset($_GET["line"]) || $_GET["line"] == "1")
	$tables["planet_osm_line"] =[];
if(!isset($_GET["polygon"]) || $_GET["polygon"] == "1")
	$tables["planet_osm_polygon"] =[];
if(!isset($_GET["point"]) || $_GET["point"] == "1")
	$tables["planet_osm_point"] =[];

foreach ($tables as $key=>$v){
	$query = "SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = '$key' AND table_catalog = '$DB_NAME'";
	$result = pg_query($query) or die('Query failed: ' . pg_last_error());
	while ($line = pg_fetch_array($result, null, PGSQL_ASSOC)) 
		if(!in_array($line["column_name"],["way","tags"]))
			array_push($tables[$key], $line["column_name"]);
}
$resArr = [];

foreach ($tables as $key=>$cols){
	$prefix= "W";
	if($key == "planet_osm_point")
		$prefix = "N";
	$q = "SELECT 
	*,
	hstore_to_json_loose(tags) as tags_json,
	st_asgeojson(ST_Transform(way,4326)) as geojson,
	st_asgeojson(ST_Centroid(ST_Transform(way,4326))) as center,
	st_asgeojson(ST_Envelope(ST_Transform(way,4326))) as bbox 
	FROM $key WHERE ";
	$exps = [];
	foreach($tags as $tag => $vals){
		$tag = mb_strtolower($tag);
		if(!is_array($vals) || count($vals) == 0)
			continue;
		$vals = array_filter($vals, function($element) {
			return !empty($element);
		});
		$vals = array_unique($vals);
			
		foreach($vals as $k=>$vl)
			$vals[$k] = "'".addcslashes(mb_strtolower(trim($vl)),"'\"")."'";
		$col = "";
		if(in_array($tag,$cols))
			$col = "\"$tag\"";
		else
			$col ="\"tags\"->'$tag'";
		$v = "";
		$oper = "";
		if(count($vals) > 1){
			$v = implode(",",$vals);
			$oper = "IN";
		}else{
			$oper ="=";
			$v = $vals[0];
		}
		array_push($exps," lower($col) $oper ($v) ");
	}
	$query = "$q ".implode("OR",$exps);
	$result = pg_query($query) or die('Query failed: ' . pg_last_error());
	
	$tempArr = [];
	while ($line = pg_fetch_array($result, null, PGSQL_ASSOC)){
		$center = json_decode($line["center"],true);
		$arr = [
			"osm_type"=>$prefix == "W"?"way":"node",
			"display_name"=>$line["name"],
			"osm_id"=>$line["osm_id"],
			"lat"=>$center["coordinates"][1],
			"lon"=>$center["coordinates"][0],
			"boundingbox"=>getBBoxFromGeoJson($line["bbox"]),
			"geojson"=>json_decode($line["geojson"],true)
		];
		$tagsCol = json_decode($line["tags_json"],true);
		
		$exclude_cols = ["osm_id","way_area","way","tags","z_order","ref","name","layer","bbox","center","tags_json","geojson"];
		foreach($line as $k=>$v){
			if(!empty($v) && !in_array($k,$exclude_cols))
				$tagsCol[$k]= $v;
		}
		$arr["extratags"] = $tagsCol;
		$tempArr[] = $arr;
	}
	$resArr = array_merge($tempArr,$resArr);
}

//order by name resArr
usort($resArr,function($a,$b){
	if(empty($a["display_name"]) && !empty($b["display_name"]))
		return 1;
	if(!empty($a["display_name"]) && empty($b["display_name"]))
	
		return -1;
	return 0;
});
echo json_encode($resArr);

}catch(Exception $e){
	http_response_code(505);
	die;
}

pg_free_result($result);

pg_close($dbconn);
else:
echo "Только POST запросы!";
endif;
?>
