const osrmTextInstructions = require('osrm-text-instructions')("v5");
const express = require('express');

var app = express();

app.use(express.json({limit: '50mb'}));
app.use(function(req, res, next) {	
  res.header("Access-Control-Allow-Origin", "*");
  res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
  next();
});

app.post('/', function(request, response){
	let arr = [];
	request.body.forEach(function(leg) {
	  leg.steps.forEach(function(step) {
		arr.push(osrmTextInstructions.compile('ru', step, {}));
	  });
	});
   response.send(arr);    
});

app.listen(8080);
console.log("Server started at port 8080");