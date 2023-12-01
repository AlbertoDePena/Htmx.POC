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

document.addEventListener('DOMContentLoaded', function () {
    const navbarBurgers = Array.prototype.slice.call(document.querySelectorAll('.navbar-burger'), 0);
    const asideBurgers = Array.prototype.slice.call(document.querySelectorAll('.aside-burger'), 0);

    navbarBurgers.forEach(function (navbarBurger) {
        navbarBurger.addEventListener('click', function () {
            const navbarMenu = document.getElementById(navbarBurger.dataset.target);

            navbarBurger.classList.toggle('is-active');
            navbarMenu.classList.toggle('is-active');
        });
    });

    asideBurgers.forEach(function (asideBurger) {
        asideBurger.addEventListener('click', function () {
            const aside = document.getElementById(asideBurger.dataset.target);
            const main = document.getElementById(aside.dataset.target);

            aside.classList.toggle('is-sidebar-hidden');
            main.classList.toggle('is-sidebar-hidden');
        });
    });
    
    let isMobile = false;
    window.addEventListener('resize', function () {
        if (window.outerWidth > 1024) {
            isMobile = false;
        } else {
            isMobile = true;
        }

        asideBurgers.forEach(function (asideBurger) {
            const aside = document.getElementById(asideBurger.dataset.target);
            const main = document.getElementById(aside.dataset.target);

            if (isMobile) {
                aside.classList.add('is-sidebar-hidden');
                main.classList.add('is-sidebar-hidden');
            } else {
                aside.classList.remove('is-sidebar-hidden');
                main.classList.remove('is-sidebar-hidden');
            }
        });
    });
});