function onDataBound(e) {
    console.log("DataBound event fired");
}

function onEditClick(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    window.location.href = '/JobTitles/Edit/' + dataItem.Id;
}