function onDataBound(e) {
    var grid = e.sender;
    var data = grid.dataSource.view();

    for (var i = 0; i < data.length; i++) {
        var item = data[i];
        var photoContainer = $("#photo_" + item.Id);

        if (item.PhotoUrl) {
            photoContainer.html('<img src="' + item.PhotoUrl + '" style="width:50px;height:50px;border-radius:50%;object-fit:cover;" alt="Employee Photo" />');
        } else {
            photoContainer.html('<span class="text-muted">No Photo</span>');
        }
    }
}

function detailsClick(e) {
    e.preventDefault();
    var grid = $("#grid").data("kendoGrid");
    var dataItem = grid.dataItem($(e.currentTarget).closest("tr"));

    if (dataItem) {
        window.location.href = '/Employees/Details/' + dataItem.Id;
    }
}


$(document).ready(function () {

});