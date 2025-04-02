document.addEventListener('DOMContentLoaded', () => {
    htmx.config.historyEnabled = false;

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