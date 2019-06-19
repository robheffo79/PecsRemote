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
            login();
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
                    if (loginOldContent !== null) {
                        $('.main-container').empty().append(loginOldContent);
                        loginOldContent = null;
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
    }
};

$(document).ready(function () {
    adminApp.login();
});