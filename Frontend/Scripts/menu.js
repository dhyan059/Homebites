// Menu page helpers.
(function (window) {
	window.HomebitesMenu = {
		matchesCategory(meal, category) {
			return category === 'all' || String(meal.categoryId) === String(category);
		}
	};
})(window);
