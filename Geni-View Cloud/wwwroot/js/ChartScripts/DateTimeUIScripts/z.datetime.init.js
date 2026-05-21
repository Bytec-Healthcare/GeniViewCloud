// Set Localization...
$.datetimepicker.setLocale('en');

// Helper: format a Date as yyyy/MM/dd HH:mm (matches picker format Y/m/d H:i)
function gvDtNowFormatted() {
    var d = new Date();
    var pad = function(n) { return n < 10 ? '0' + n : n; };
    return d.getFullYear() + '/' + pad(d.getMonth() + 1) + '/' + pad(d.getDate()) + ' ' + pad(d.getHours()) + ':' + pad(d.getMinutes());
}
function gvDateOnlyFormatted() {
    var d = new Date();
    var pad = function(n) { return n < 10 ? '0' + n : n; };
    return d.getFullYear() + '/' + pad(d.getMonth() + 1) + '/' + pad(d.getDate());
}

// Default 7 days range.
var beginDate = new Date();
beginDate.setDate(beginDate.getDate() - 7);

$('#BeginDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: beginDate,
    step: 5,
    maxDate: gvDateOnlyFormatted(),
    maxTime: gvDtNowFormatted()
});

$('#EndDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: new Date(),
    step: 5,
    maxDate: gvDateOnlyFormatted(),
    maxTime: gvDtNowFormatted()
});

//var Bday = dateFormat(new Date($('#fBeginDate').val()), 'yyyy/mm/dd HH:MM');

$('#fBeginDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: $('#fBeginDate').val(),
    step: 5,
    maxDate: gvDateOnlyFormatted(),
    maxTime: gvDtNowFormatted()
});

$('#fEndDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: $('#fEndDate').val(),
    step: 5,
    maxDate: gvDateOnlyFormatted(),
    maxTime: gvDtNowFormatted()
});