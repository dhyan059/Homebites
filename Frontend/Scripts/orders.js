// Order page helpers.
(function (window) {
	window.HomebitesOrders = {
		statusLabel(status) {
			return String(status || '').replace(/([a-z])([A-Z])/g, '$1 $2');
		}
	};
})(window);
