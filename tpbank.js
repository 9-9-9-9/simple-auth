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
var rate = 0.006;

var loop = function() {
    updateRemainingMoney();
    
    updateSalary();

    setTimeout(loop, 10);
}

var updateRemainingMoney = function() {
    var pMoney = $('p.money');
    var overview = $('app-account-overview').children().eq(1).children().eq(0).children();
    var divMoney = $('div.money');

    if (pMoney.length == 0 && overview.length == 0 && divMoney.length == 0) {
        return;
    }

    pMoney.text(rms);
    overview.eq(1).text(rms);
    overview.eq(3).text(rms);
    divMoney.text(rms);
}

var updateSalary = function() {
    var li = $('ul.list-transaction > li > app-transaction-item > div > ul > li');

    if (li.length < 5) {
        return;
    }

    var remaining = rmn;
    
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

        var postTrans = remaining;

        if (m.indexOf('tra lai tien gui') > -1) {
            // Xu ly lai~
            var profit = postTrans / (1 + rate) * rate;
            c = Math.floor(profit);
            hookUpContent(hookUp, p, c);
        }

        var preTrans = remaining + (p ? -1 : 1) * c;

        remaining = preTrans;
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

    //console.log('Hookup success with value ' + hookUpContent);
}

setTimeout(loop, 50);