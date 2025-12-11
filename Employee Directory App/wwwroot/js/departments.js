function onDataBound(e) {
    console.log("DataBound event fired");
    var grid = this;
}

function onEditClick(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    window.location.href = '/Departments/Edit/' + dataItem.Id;
}