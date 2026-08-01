// Set Localization...
$.datetimepicker.setLocale("en");

// Helper: format a date as yyyy/MM/dd (used as maxDate to block future dates)
function gvDateOnlyFormatted() {
  var d = new Date();
  var pad = function (n) {
    return n < 10 ? "0" + n : n;
  };
  return d.getFullYear() + "/" + pad(d.getMonth() + 1) + "/" + pad(d.getDate());
}

// gvInitDatePickers() is called from each view's setDefaultDatesAndLoad() AFTER
// the field values have been set. Initializing the picker with the already-correct
// value means the calendar always opens to the right month on first click.
// Note: maxTime is intentionally omitted — it causes xdsoft to anchor the calendar
// view to today's month rather than the value's month.
function gvInitDatePickers() {
  var $begin = $("#BeginDate");
  var $end = $("#EndDate");

  var beginVal = $begin.val();
  var endVal = $end.val();

  if ($begin.length) {
    $begin.datetimepicker({
      mask: "9999/19/39 29:59",
      dayOfWeekStart: 1,
      lang: "en",
      value: beginVal || false,
      step: 5,
      maxDate: gvDateOnlyFormatted(),
    });
  }

  if ($end.length) {
    $end.datetimepicker({
      mask: "9999/19/39 29:59",
      dayOfWeekStart: 1,
      lang: "en",
      value: endVal || false,
      step: 5,
      maxDate: gvDateOnlyFormatted(),
    });
  }
}

// Keep as a no-op alias so any existing callers don't break
function gvSyncPickerValues() {}

// gvInitFDatePickers() is called from History/Events views' document.ready AFTER
// field values are set from the model. Same pattern as gvInitDatePickers().
// Note: maxTime is intentionally omitted — see gvInitDatePickers comment above.
function gvInitFDatePickers() {
  var $begin = $("#fBeginDate");
  var $end = $("#fEndDate");

  var beginVal = $begin.val();
  var endVal = $end.val();

  if ($begin.length) {
    $begin.datetimepicker({
      mask: "9999/19/39 29:59",
      dayOfWeekStart: 1,
      lang: "en",
      value: beginVal || false,
      step: 5,
      maxDate: gvDateOnlyFormatted(),
    });
  }

  if ($end.length) {
    $end.datetimepicker({
      mask: "9999/19/39 29:59",
      dayOfWeekStart: 1,
      lang: "en",
      value: endVal || false,
      step: 5,
      maxDate: gvDateOnlyFormatted(),
    });
  }
}

//var Bday = dateFormat(new Date($('#fBeginDate').val()), 'yyyy/mm/dd HH:MM');
// NOTE: fBeginDate/fEndDate pickers are initialized by gvInitFDatePickers(),
// called from each view's document.ready after field values are set.
