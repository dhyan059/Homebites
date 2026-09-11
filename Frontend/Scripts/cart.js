// Cart page helpers.
(function (window) {
	window.HomebitesCart = {
		totals(cart) {
			var subtotal = (cart || []).reduce(function (sum, item) {
				return sum + Number(item.discountPrice || item.price || 0) * Number(item.quantity || 0);
			}, 0);
			var delivery = subtotal >= 300 ? 0 : 40;
			var tax = Math.round(subtotal * 0.05);
			return { subtotal: subtotal, delivery: delivery, tax: tax, total: subtotal + delivery + tax };
		}
	};
})(window);
