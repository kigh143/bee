<p align="center">
  <a href="http://gulpjs.com">
    <img height="257" width="114" src="https://raw.githubusercontent.com/gulpjs/artwork/master/gulp-2x.png">
  </a>
  <p align="center">BEE - the realtime Back End Engine </p>
</p>

## What is bee?

## Documentation

## Example in javascript using `bee.js`

This file will give you a taste of what bee does.

```js
var colony = new Colony("https://kop.bee.com","KINGSOFPREDICTION");
var bee = colony.newBee();

//structure some nector
var nector = {
    Games:[{
        _asc_: "StartTime",
        _pg_:"1 10",
        _w:{
            _$StartTime_gt : bee.now
        }
    }]
};

//get some honey from your nector
bee.get(nector,function(honey){
    console.log(honey.Games);
});

//this will give us a least of games whose StartTime is greater than the current time
//ordered by ascending order according to their startime
//and this ill be the first page of results fetched having a maximum of 10 games