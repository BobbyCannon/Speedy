var Speedy = Speedy ||
{
	properties: [
		'cellIndex', 'checked', 'className', 'disabled', 'href', 'id', 'multiple', 'name', 'nodeType', 'readOnly',
		'rowIndex', 'selected', 'src', 'tagName', 'textContent', 'value'
	],
	autoId: 1,
	ignoredTags: ['script'],
	resultElementId: 'speedyResult',
	getElementHost: function(frameId) {
		// Check to see if the element is an iFrame. If so get it document, else we'll just return the current element
		var hostDocument = frameId ? document.getElementById(frameId) : document;
		return (hostDocument.contentDocument ? hostDocument.contentDocument : hostDocument.contentWindow ? hostDocument.contentWindow.document : hostDocument);
	},
	triggerEvent: function(element, frameId, eventName, values) {
		var event;
		var hostDocument = Speedy.getElementHost(frameId);
		if (hostDocument.createEvent) {
			event = hostDocument.createEvent('HTMLEvents');
			event.initEvent(eventName, true, true);
		}

		if (event) {
			//event.eventName = eventName;
			for (var i = 0; i < values.length; i++) {
				event[values[i].key] = values[i].value;
			}
			element.dispatchEvent(event);
		} else {
			element.fireEvent('on' + eventName, event);
		}
	},
	getElementLocation: function (id, frameId) {
		var hostDocument = Speedy.getElementHost(frameId);
		var element = hostDocument.getElementById(id);
		var box = element.getBoundingClientRect();
		var borderWidth = (window.outerWidth - window.innerWidth) / 2;
		var x = window.screenX + borderWidth;
		var y = window.screenY + window.outerHeight - window.innerHeight - borderWidth;
		var top = Math.round(y + box.top);
		var left = Math.round(x + box.left);
		return JSON.stringify({ x: left, y: top });
	},
	getElementsFromHost: function (host, frameId, forParentId) {
		var response = [];
		var processedFrames = [];
		var allElements = host.getElementsByTagName('*');
		var i;

		// Add element IDs so we can build element hierarchy.
		for (i = 0; i < allElements.length; i++) {
			var currentId = Speedy.getValueFromElement(allElements[i], 'id');
			if (currentId === null || currentId === undefined || currentId === '') {
				allElements[i].id = 'speedy-' + Speedy.autoId++;
			}
		}

		for (i = 0; i < allElements.length; i++) {
			var element = allElements[i];
			var tagName = (element.tagName).toLowerCase();

			if (element.id === Speedy.resultElementId) {
				continue;
			}

			if (Speedy.ignoredTags.contains(tagName)) {
				continue;
			}

			var elementId = Speedy.getValueFromElement(element, 'id');
			var elementName = Speedy.getValueFromElement(element, 'name') || '';
			var parentId = Speedy.getValueFromElement(element.parentNode, 'id') || frameId || '';

			if (forParentId !== undefined && parentId !== forParentId) {
				continue;
			}

			var item = {
				id: elementId,
				parentId: parentId,
				name: elementName,
				tagName: tagName,
				attributes: [],
				frameId: frameId
			};

			item.width = element.offsetWidth;
			item.height = element.offsetHeight;

			for (var j = 0; j < element.attributes.length; j++) {
				var attribute = element.attributes[j];

				if (attribute.nodeName === undefined || attribute.nodeName.length <= 0) {
					continue;
				}

				if (item[attribute.nodeName]) {
					continue;
				}

				item.attributes.push(attribute.nodeName);
				item.attributes.push(attribute.nodeValue);
			}

			for (var k = 0; k < Speedy.properties.length; k++) {
				var name = Speedy.properties[k];

				if (item[name] || name === 'textContent') {
					continue;
				}

				if (element[name] !== null && element[name] !== undefined) {
					item.attributes.push(name);
					if (typeof element[name] === 'string') {
						item.attributes.push(element[name]);
					} else {
						item.attributes.push(JSON.stringify(element[name]));
					}
				}
			}
			
			response.push(item);

			try {
				if (item.tagName.toLowerCase() === 'iframe' && !processedFrames.contains(item.id)) {
					processedFrames.push(item.id);
					var itemHost = (element.contentDocument ? element.contentDocument : element.contentWindow.document);
					var children = Speedy.getElementsFromHost(itemHost, item.id, forParentId);
					for (i = 0; i < children.length; i++) {
						response.push(children[i]);
					}
				}
			} catch (ex) {
				//console.log(ex.message);
			}
		}

		return response;
	},
	getElements: function (forParentId, frameId) {
		var host = Speedy.getElementHost(frameId);
		return Speedy.getElementsFromHost(host, frameId, forParentId);
	},
	getElementValue: function (elementId, frameId, name) {
		var hostDocument = Speedy.getElementHost(frameId);
		var element = hostDocument.getElementById(elementId);
		if (element === undefined || element === null) {
			return null;
		}

		return Speedy.getValueFromElement(element, name);
	},
	getSelectText: function (elementId, frameId) {
		var hostDocument = Speedy.getElementHost(frameId);
		var element = hostDocument.getElementById(elementId);
		if (element.selectedIndex === -1) {
			return null;
		}

		return element.options[element.selectedIndex].text;
	},
	getValueFromElement: function(element, name) {
		try {
			if (element === undefined || element === null) {
				return null;
			}

			var value = element[name];
			if ((value === null || value === undefined) || (element.nodeType === 1 && typeof (value) === 'object')) {
				value = element.attributes[name];
			}
			
			if (value !== null && value !== undefined) {
				if (value.value !== undefined && value.value !== null) {
					return value.value.toString();
				}
				
				if (value.nodeValue !== undefined && value.nodeValue !== null) {
					return value.nodeValue.toString();
				}

				return value.toString();
			}
		} catch (e) {
			return null;
		}

		return null;
	},
	setElementValue: function (elementId, frameId, name, value) {
		var hostDocument = Speedy.getElementHost(frameId);
		var element = hostDocument.getElementById(elementId);
		if (element === undefined || element === null) {
			return;
		}

		if (Speedy.properties.contains(name)) {
			element[name] = value;
		} else {
			element.setAttribute(name, value);
		}
	},
	setSelectText: function (elementId, frameId, value) {
		var hostDocument = Speedy.getElementHost(frameId);
		var i, element = hostDocument.getElementById(elementId);

		for (i = 0; i < element.options.length; i++) {
			if (element.options[i].text === value) {
				element.options[i].selected = true;
				Speedy.triggerEvent(element, frameId, 'change', []);
				return;
			}
		}

		for (i = 0; i < element.options.length; i++) {
			if (element.options[i].text.lastIndexOf(value, 0) === 0) {
				element.options[i].selected = true;
				Speedy.triggerEvent(element, frameId, 'change', []);
				return;
			}
		}
	},
	removeElement: function (elementId, frameId) {
		var hostDocument = Speedy.getElementHost(frameId);
		var element = hostDocument.getElementById(elementId);
		element.parentNode.removeChild(element);
	},
	removeElementAttribute: function (elementId, frameId, name) {
		var hostDocument = Speedy.getElementHost(frameId);
		var element = hostDocument.getElementById(elementId);
		element.removeAttribute(name);
	},
	rightClick: function (elementId, frameId) {
		var hostDocument = Speedy.getElementHost(frameId);
		var element = hostDocument.getElementById(elementId);
		var evt = element.ownerDocument.createEvent('MouseEvents');
		var rightClickButtonCode = 2;

		evt.initMouseEvent('contextmenu', true, true, element.ownerDocument.defaultView, 1,
			0, 0, 0, 0, false, false, false, false, rightClickButtonCode, null);

		if (document.createEventObject) {
			// dispatch for IE
			return element.fireEvent('onclick', evt);
		} else {
			// dispatch for firefox + others
			return !element.dispatchEvent(evt);
		}
	},
	runScript: function(script) {
		// decode the script.
		script = script
			.replace(/&quot;/g, '"')
			.replace(/&#39;/g, "'")
			.replace(/&lt;/g, '<')
			.replace(/&gt;/g, '>')
			.replace(/&amp;/g, '&');

		// Attempt to find the result element.
		var resultElement = document.getElementById(Speedy.resultElementId);

		// Check to see if the element was found.
		if (resultElement === undefined || resultElement === null) {
			// Create new element (input "hidden") then set the value.
			resultElement = document.createElement('input');
			resultElement.id = Speedy.resultElementId;
			resultElement.type = 'hidden';

			if (document.body && document.body.appendChild) {
				document.body.appendChild(resultElement);
			}
		}

		try {
			// Clear the results first then set the new value.
			resultElement.value = '';
			resultElement.value = eval(script);
		} catch (error) {
			// Something went wrong so update the result with the error.
			console.log(error.message);
			resultElement.value = error.message;
		}
	}
};

Array.prototype.contains = function (obj) {
	for (var i = 0; i < this.length; i++) {
		if (this[i] === obj) {
			return true;
		}
	}

	return false;
};