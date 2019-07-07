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
            $.ajax({ url: 'components/footer.html', crossDomain: true }),
            $.ajax({ url: 'api/system/updates', crossDomain: true}),
        ).done(function (header, main, footer, updates) {
            $('.main-container').append(header[0]).append(main[0]).append(footer[0]);
            $('#btnTitle').on('click', function () { adminApp.home(); });
            $('#btnHome').on('click', function () { adminApp.home(); });
            $('#btnMedia').on('click', function () { adminApp.media(); });
            $('#btnPlaylists').on('click', function () { adminApp.playlists(); });
            $('#btnSetup').on('click', function () { adminApp.setup(); });
            $('#btnHistory').on('click', function () { adminApp.history(); });
            $('#btnLogout').on('click', function () { adminApp.logout(); });
            $('#btnUpdates').on('click', function () { adminApp.applyUpdates(); });

            if (updates[0] >= 0) {
                $('#txtUpdateCount').html = updates[0];
                $('#btnUpdates').removeAttr('hidden');
            }

            adminApp.home();
        });

        adminApp.popLoader();
    },

    home() {
        adminApp.setMainPanel('btnHome', 'components/main.html');
    },

    currentMedia: null,

    media() {
        adminApp.setMainPanel('btnMedia', 'components/media/main.html', function () {
            var tplMediaItem = Handlebars.compile($('#tplMediaItem').html());
            var tplMediaPlayer = Handlebars.compile($('#tplMediaPlayer').html());
            var pnlMediaItems = $('#pnlMediaItems');
            var pnlMediaPlayer = $('#pnlMediaPlayer');

            adminApp.pushLoader();

            $.ajax({ url: '/api/media', crossDomain: true }).done(function (media) {
                if (media.length > 0) {
                    currentMedia = media;
                    pnlMediaItems.empty();

                    $.each(media, function () {
                        var item = tplMediaItem(this);
                        pnlMediaItems.append(item);
                    });

                    $('.media-link').on('click', function () {
                        var mediaId = $(this).closest('.media-item').data('media-id');
                        var mediaItem = currentMedia.find(function(m) {
                            return m.id === mediaId;
                        });
                        var player = tplMediaPlayer(mediaItem);
                        pnlMediaPlayer.empty();
                        pnlMediaPlayer.append(player);
                    });
                }

                adminApp.popLoader();
            });

            $('#txtUrl').on('change', function () {
                var input = $(this);
                $.ajax({ url: '/api/media/suggest?q=' + encodeURI(input.val()), crossDomain: true}).done(function (result) {
                    if (result !== 'undefined' && result !== null) {
                        $('#txtTitle').val(result.title);
                    }
                });
            })

            $('#btnAddMedia').on('click', function () {
                var addModal = $('#modAddMedia');
                addModal.modal();
                addModal.modal('show');
            });
        });
    },

    playlists() {
        alert('Not Implemented.');
    },

    setup() {
        adminApp.setMainPanel('btnSetup', 'components/setup/main.html');
    },

    history() {
        alert('Not Implemented.');
    },

    logout() {
        window.sessionStorage.removeItem('token');
        adminApp.login();
    },

    applyUpdates() {
        var dots = 1;
        var previousUptime = 0;

        adminApp.setMainPanel('btnUpdates', 'components/updates/main.html', function () {
            var confirmModal = $('#modUpdateConfirm');

            $('#btnCancelUpdates').on('click', function () {
                confirmModal.modal('hide');
                adminApp.home();
            });

            $('#btnApplyUpdates').on('click', function () {
                confirmModal.modal('hide');

                var updateModal = $('#modUpdateProgress');
                updateModal.modal({ backdrop: 'static', keyboard: false });
                updateModal.modal('show');

                $.ajax({ url: 'api/system/update', crossDomain: true, method: 'POST' }).done(function () {
                    var checkHandle = window.setInterval(function () {

                        $.ajax({ url: 'api/system/status', crossDomain: true, timeout: 500 }).done(function (data) {
                            if (data.uptime < previousUptime) {
                                clearInterval(checkHandle);
                                updateModal.modal('hide');
                                window.sessionStorage.removeItem('token');
                                location.reload(true);
                            } else {
                                previousUptime = data.uptime;
                                $('.updates').empty().append('<p>Applying Updates. Please Wait' + ('.'.repeat(dots)) + '</p>');
                                if (++dots >= 4) {
                                    dots = 1;
                                }
                            }
                        }).fail(function (jqXHR, textStatus, errorThrown) {
                            $('.updates').empty().append('<p>Rebooting. Please Wait' + ('.'.repeat(dots)) + '</p>');
                            if (++dots >= 4) {
                                dots = 1;
                            }
                        });

                    }, 1000);
                });
            });

            confirmModal.modal();
            confirmModal.modal('show');
        });
    },

    setMainPanel(buttonId, url, then) {
        $.ajax({ url: url, crossDomain: true }).done(function (html) {
            $('#pnlMain').empty().append(html);

            var active = $('#navMain').find('.active');
            active.find('.sr-only').remove();
            active.removeClass('active');

            var btn = $('#' + buttonId);
            btn.addClass('active');
            btn.append('<span class="sr-only"> (current)</span>');

            if (then !== undefined) {
                then();
            }
        });
    }
};

$(document).ready(function () {
    adminApp.init();
});