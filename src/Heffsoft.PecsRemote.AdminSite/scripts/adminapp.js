const adminApp = {
    loaderDepth: 0,

    pushLoader() {
        this.loaderDepth++;
        if (this.loaderDepth > 0) {
            $('.loader').removeClass('invisible');
        }
    },

    popLoader() {
        this.loaderDepth--;
        if (this.loaderDepth <= 0) {
            this.loaderDepth = 0;
            $('.loader').addClass('invisible');
        }
    },

    init() {
        adminApp.pushLoader();

        // Setup Global Ajax Handlers
        $(document).ajaxSend(function (event, jqxhr, settings) {
            var token = window.sessionStorage.getItem('token');
            if (token !== null)
                jqxhr.setRequestHeader('Authorization', 'Token ' + token);
        });

        $(document).ajaxError(function (event, jqxhr, settings, error) {
            if (jqxhr.statusCode == 401) {
                this.login();
            }
        });

        var token = window.sessionStorage.getItem('token');
        if (token === null) {
            adminApp.login();
        } else {
            adminApp.main();
        }

        adminApp.popLoader();
    },

    loginOldContent: null,

    login() {
        adminApp.pushLoader();
        $.ajax({ url: 'components/login.html', crossDomain: true }).done(function (template) {
            loginOldContent = $('.main-container').children().detach();
            $('.main-container').empty().append(template);
            $('#sign-in').on('click', function () {
                $('.login-form div.alert-danger').addClass('invisible');

                var postObject = {
                    username: $('#loginUsername').val(),
                    password: $('#loginPassword').val()
                };

                $.ajax({
                    method: 'POST',
                    url: 'api/users/authenticate',
                    crossDomain: true,
                    data: JSON.stringify(postObject),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json'
                }).done(function (response) {
                    window.sessionStorage.setItem('token', response.token);
                    if (loginOldContent !== null && loginOldContent.length > 0) {
                        $('.main-container').empty().append(loginOldContent);
                        loginOldContent = null;
                    } else {
                        loginOldContent = null;
                        adminApp.main();
                    }
                }).fail(function (response) {
                    var message = "An error has occurred.";

                    switch (response.statusCode) {
                        case 400:
                        case 401:
                            message = 'Invalid Username or Password.';
                            break;

                        case 404:
                            message = 'The API service was not found.';
                            break;

                        case 500:
                            message = 'An error occured in the API service.';
                            break;
                    }

                    $('.login-form div.alert-danger').removeClass('invisible').text(message);
                });
            });
            adminApp.popLoader();
        });
    },

    main() {
        adminApp.pushLoader();
        $('.main-container').empty()

        // Load Header
        $.when(
            $.ajax({ url: 'components/header.html', crossDomain: true }),
            $.ajax({ url: 'components/main.html', crossDomain: true }),
            $.ajax({ url: 'components/footer.html', crossDomain: true })
        ).done(function (header, main, footer) {
            $('.main-container').append(header[0]).append(main[0]).append(footer[0]);
            $('#btnTitle').on('click', function () { adminApp.home(); });
            $('#btnHome').on('click', function () { adminApp.home(); });
            $('#btnPlaylists').on('click', function () { adminApp.playlists(); });
            $('#btnSetup').on('click', function () { adminApp.setup(); });
            $('#btnHistory').on('click', function () { adminApp.history(); });
            $('#btnLogout').on('click', function () { adminApp.logout(); });
        });

        adminApp.popLoader();
    },

    home() {
        alert('Not Implemented.');
    },

    playlists() {
        alert('Not Implemented.');
    },

    setup() {
        alert('Not Implemented.');
    },

    history() {
        alert('Not Implemented.');
    },

    logout() {
        window.sessionStorage.removeItem('token');
        adminApp.login();
    }
};

$(document).ready(function () {
    adminApp.init();
});