document.addEventListener('DOMContentLoaded', () => {
    
    // override config to handle htmx quirks
    htmx.config.disableInheritance = true;
    htmx.config.historyEnabled = false;
    htmx.config.historyCacheSize = 0;

    const activeClass = 'is-active';
    const navbarBurger = document.getElementById('NavbarBurger');

    if (navbarBurger) {
        const mainNavbar = document.getElementById(navbarBurger.dataset.target);

        if (mainNavbar) {
            navbarBurger.addEventListener('click', () => {
                navbarBurger.classList.toggle(activeClass);
                mainNavbar.classList.toggle(activeClass);
            });
        }
    }    
});