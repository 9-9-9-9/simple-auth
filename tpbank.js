setTimeout(reDraw, 500);

var reDraw = function() {
    var rms = '228,938,873 VND';
    var rmn = 228938873;
    $('p.money').text(rms);
    var rml = $('app-account-overview').children().eq(1).children().eq(0).children();
    rml.eq(1).text(rms);
    rml.eq(3).text(rms);
    
    var li = $('ul.list-transaction > li > app-transaction-item > div > ul > li');
    
    $.each(li, function(i, v){
        var _ = $(v);
        var m = _.find('div.transaction-name').text().toLowerCase();
        var p = _.find('span.plus').length > 0;
        var c = parseInt(_.find('div.item-right').find('span').text().trim().split(' ')[1].split(',').join(''));
        console.log(m + ' > ' + (p ? '+' : '-') + ' > ' + c);
    });
}

