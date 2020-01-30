var rms = '128,938,873 VND';
var rmn = 128938873;

var reDraw = function() {
    updateRemainingMoney();
    
    updateSalary();

    registerButtons();
}

var updateRemainingMoney = function() {
    var pMoney = $('p.money');
    var overview = $('app-account-overview').children().eq(1).children().eq(0).children();
    if (pMoney.length == 0 && overview.length == 0) {
        setTimeout(updateRemainingMoney, 10);
        return;
    }

    pMoney.text(rms);
    overview.eq(1).text(rms);
    overview.eq(3).text(rms);
}

var updateSalary = function() {
    var li = $('ul.list-transaction > li > app-transaction-item > div > ul > li');

    if (li.length < 5) {
        setTimeout(updateSalary, 500);
        return;
    }
    
    $.each(li, function(i, v){
        var _ = $(v);
        var m = _.find('div.transaction-name').text().toLowerCase();
        var p = _.find('span.plus').length > 0;

        var hookUp = _.find('div.item-right').find('span');
        var c = parseInt(hookUp.text().trim().split(' ')[1].split(',').join(''));
        console.log(m + ' > ' + (p ? '+' : '-') + ' > ' + c);

        if (c === 25000000 || c === 24467618) {
            hookUp.html(' + 32,000,000 &nbsp;VND');
        } else if (c === 1898999) {
            hookUp.html(' + 3,851,230 &nbsp;VND');
        }
    });
}

var delayedReDraw  = function(t) {
    setTimeout(reDraw, t === undefined ? 500 : t);
}

var registerButtons = function() {
    $.each($('div.tab-main > div.nav-tab'), function(i, v) {
        $(v).click(function(){
            delayedReDraw(50);
        });
    });
}

delayedReDraw();