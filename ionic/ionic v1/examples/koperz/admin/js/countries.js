$(document).ready(function(){

	$('.data-table').dataTable({
		"bJQueryUI": true,
		"sPaginationType": "full_numbers",
		"sDom": '<""l>t<"F"fp>'
	});

	$('select').select2();

	$('input[type=checkbox],input[type=radio],input[type=file]').uniform();

	pageView = new Vue({
	  el: '#content',
	  data: {
	    form: {
				Name: "",
				Code: "",
				ShortName : "",
				Flag: ""
			},
			list: []
	  }
	});

	//get a list of countries
	bee.get({Countries:[],_errors:[]},function(hny){
		console.log("countries honey: ", hny);
	});

});
