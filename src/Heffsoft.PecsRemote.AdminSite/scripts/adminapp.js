const adminApp = {
    token: null,
    login() {
        $('.loader').removeClass('invisible');
        $.ajax({ url: 'components/login.html', crossDomain: true }).done(function (template) {
            $('.main-container').append(template);
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
                }).done(function (token) {
                    this.token = token;
                }).fail(function (response) {
                    if (response.statusCode == 404) {
                        $('.login-form div.alert-danger').removeClass('invisible').text("The API service was not found.");
                    } else if (response.statusCode == 500) {
                        $('.login-form div.alert-danger').removeClass('invisible').text("An error occured in the API service.");
                    } else if (response.statusCode == 401) {
                        $('.login-form div.alert-danger').removeClass('invisible').text("Invalid Username or Password.");
                    } else {
                        $('.login-form div.alert-danger').removeClass('invisible').text("Unable to connect to API service.");
                    }
                });
            });
            $('.loader').addClass('invisible');
        });
    }
};

$(document).ready(function () {
    adminApp.login();
});