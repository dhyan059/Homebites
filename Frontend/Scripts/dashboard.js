// Dashboard page helpers.
(function (window) {
	window.HomebitesDashboard = {
		formatCount(value) {
			return Number(value || 0).toLocaleString('en-IN');
		}
	};
})(window);
