namespace MSDEV {
    export function LoadForecast() {
        debugger;

        var postalCode = Xrm.Page.getAttribute("msdev_postalcode").getValue();


        if (!!postalCode && /(^\d{5}$)/.test(postalCode)) {
            var forecast = <any>Xrm.Page.ui.controls.get("WebResource_forecast");
            forecast.setSrc(`http://weathersticker.wunderground.com/weathersticker/cgi-bin/banner/ban/wxBanner?bannertype=wu_clean2day_cond&zip=${postalCode}&language=EN`);
        }
    }
}