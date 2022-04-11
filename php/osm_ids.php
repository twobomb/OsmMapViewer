<?PHP
require_once "./db_config.php";


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
$queryGen = [];
foreach ($tables as $key=>$cols){
	$prefix= "W";
	if($key == "planet_osm_point")
		$prefix = "N";
	$q = "SELECT CONCAT('$prefix',osm_id) as osm_id FROM $key WHERE ";
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
	$queryGen[] = "($q ".implode("OR",$exps).")";
	
}
$resQuery = implode(" UNION ",$queryGen);
$result = pg_query($resQuery) or die('Query failed: ' . pg_last_error());
$arr = [];
while ($line = pg_fetch_array($result, null, PGSQL_ASSOC)) 
	$arr[] = $line["osm_id"];


//echo json_encode($arr);
echo implode(",",$arr);

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
