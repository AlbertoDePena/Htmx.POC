var poc = (function () {

    return {
        getQueryStringValue: function (name) {
            var searchParams = new URLSearchParams(window.location.search);
            return searchParams.get(name);
        },
        getQueryStringIntValue: function (name) {
            return Number(this.getQueryStringValue(name));
        }
    };
})();