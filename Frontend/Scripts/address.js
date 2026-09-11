// Address page helpers.
(function (window) {
	window.HomebitesAddress = {
		isWithinDeliveryZone(distanceKm) {
			return Number.isFinite(Number(distanceKm)) && Number(distanceKm) <= 12;
		}
	};
})(window);
