function imageUploadEditor(container, options) {
    container.append(`<div>
                <input type="file" name="ImageFile" id="imageFile" accept="image/*">
                <div id="uploadPreview"></div>
                </div>
            `)

    $('#imageFile').on("change", function (e) {
        var file = e.target.files[0];
        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#uploadPreview').html(`
                            <div class="photo-preview-container">
                                <p class="photo-preview-title">New Photo Preview</p>
                                <div class="photo-preview-wrapper">
                                <img src="${e.target.result}" alt="Preview" class="photo-preview-img"/>
                                </div>
                            </div>
                            `)
            }
            reader.readAsDataURL(file);
        }
    });
}
function onFormValidateField(e) {
    console.log("Validatin field:", e.field, "valid:", e.valid);
}

function removePhoto() {
    if (confirm("Are you sure you want to remove the profile photo?")) {
        $('<input>').attr({
            type: 'hidden',
            name: 'RemovePhoto',
            value: 'true'
        }).appendTo('#employeeForm');

        $('#currentProfileImage').parent().parent().hide();

        kendo.alert("Photo will be removed.");
    }
}
function onImageSelect(e) {
    var file = e.files[0];

    if (!file) return;

    if (!file.extension.match(/\.(jpg|jpeg|png|gif)$/i)) {
        kendo.alert("Invalid file type");
        return;
    }

    var reader = new FileReader();
    reader.onload = function (ev) {
        $("#uploadPreview").html(`
            <div class="photo-preview-container">
                <p class="photo-preview-title">New Photo Preview</p>
                <div class="photo-preview-wrapper">
                    <img src="${ev.target.result}" class="photo-preview-img"/>
                    </div>
                    </div>
        `);
    };
    reader.readAsDataURL(file.rawFile);
}

function onImageUploadSuccess(e) {
    console.log("Upload success:", e);

    if (e.response && e.response.length > 0) {
        var file = e.response[0];

        $("#UploadPhotoPath").val(file.newPhotoPath || "");
    }
}
function onBackClick() {
    window.location.href = '/Employees/Index';
}

function onFormSubmit(e) {
    $.ajax({
        contentType: 'application/json; charset=utf-8',
        type: 'POST',
        url: '/Employees/Edit/',
        data: {
            FirstName: FirstName, LastName: LastName, Email: Email,
            PhoneNumber: PhoneNumber, HireDate: HireDate, DepartmentId: DepartmentId,
            JobTitleId: JobTitleId, IsActive: IsActive
        },
        success: function (result) {
            alert('Податоците се успешно зачувани');
        },
        failure: function (response) {
            console.log(response);
        }
    });
}