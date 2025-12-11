function onDataBound(e) {
    console.log("DataBound event fired");
}

function onEditClick(e) {
    e.preventDefault();
    var dataItem = this.dataItem($(e.currentTarget).closest("tr"));
    window.location.href = '/EmployeeProjects/Edit/' + dataItem.Id;
}