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

    htmx.onLoad(function (target) {
        console.log('ADP TEST: ', target);

        const ctx = document.getElementById('myChart');

        if (ctx) {
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
                    datasets: [{
                        label: '# of Votes',
                        data: [12, 19, 3, 5, 2, 3],
                        borderWidth: 1
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        }    
    });


    
});