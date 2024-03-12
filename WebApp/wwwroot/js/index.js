document.addEventListener('DOMContentLoaded', function () {
    htmx.config.historyEnabled = false;

    var activeClass = 'is-active';
    var navbarBurger = document.getElementById('NavbarBurger');

    if (navbarBurger) {
        var mainNavbar = document.getElementById(navbarBurger.dataset.target);

        if (mainNavbar) {
            navbarBurger.addEventListener('click', () => {
                navbarBurger.classList.toggle(activeClass);
                mainNavbar.classList.toggle(activeClass);
            });
        }
    }
});