var rms = '228,938,873 VND';
var rmn = 228938873;

var reDraw = function() {

    updateRemainingMoney();
    
    var li = $('ul.list-transaction > li > app-transaction-item > div > ul > li');
    
    $.each(li, function(i, v){
        var _ = $(v);
        var m = _.find('div.transaction-name').text().toLowerCase();
        var p = _.find('span.plus').length > 0;
        var c = parseInt(_.find('div.item-right').find('span').text().trim().split(' ')[1].split(',').join(''));
        console.log(m + ' > ' + (p ? '+' : '-') + ' > ' + c);
    });

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