'use strict';

var app = app || null;

$(document).ready(function() {
	if (app === null || app === undefined) {
		$('#vueapp').removeAttr('v-cloak');
	}
});