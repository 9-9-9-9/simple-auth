// Begin declaration of utilities methods
var getDisplayText = function(n) {
    return n.toLocaleString(
        'en-US',
        { 
            minimumFractionDigits: 0 
        }
    );
}
// End of utils

var rmn = 128938873;
var rms = getDisplayText(rmn) + ' VND';

var loop = function() {
    updateRemainingMoney();
    
    updateSalary();

    setTimeout(loop, 10);
}

var updateRemainingMoney = function() {
    var pMoney = $('p.money');
    var overview = $('app-account-overview').children().eq(1).children().eq(0).children();

    if (pMoney.length == 0 && overview.length == 0) {
        return;
    }

    pMoney.text(rms);
    overview.eq(1).text(rms);
    overview.eq(3).text(rms);
}

var updateSalary = function() {
    var li = $('ul.list-transaction > li > app-transaction-item > div > ul > li');

    if (li.length < 5) {
        return;
    }

    $.each(li, function(i, v){
        var _ = $(v);
        var m = _.find('div.transaction-name').text().toLowerCase();
        var p = _.find('span.plus').length > 0;

        var hookUp = _.find('div.item-right').find('span');
        var c = parseInt(hookUp.text().trim().split(' ')[1].split(',').join(''));
        //console.log(m + ' > ' + (p ? '+' : '-') + ' > ' + c);

        if (c === 25_000_000 || c === 24_467_618) {
            c = 32_000_000;
            hookUpContent(hookUp, p, c);
        } else if (c === 1898999) {
            c = 3_851_230;
            hookUpContent(hookUp, p, c);
        }
    });
}

var hookUpContent = function(ins, plus, amt) {
    var hookUpContent = ' ';
    if (plus === true) {
        hookUpContent += '+';
    } else {
        hookUpContent += '-';
    }
    hookUpContent += (' ' + getDisplayText(amt) + ' &nbsp;VND');
    ins.html(hookUpContent);

    console.log('Hookup success with value ' + hookUpContent);
}

setTimeout(loop, 50);