//var dataTable;

//$(document).ready(function () {
//    loadDataTable();
//});
//function loadDataTable() {
//    dataTable = $('#tblData').DataTable({
//        "ajax": { url: '/TeamManager/Player/getall' + teamId },
//        "columns": [
//            { data: 'firstName', "width": "25%" },
//            { data: 'lastName', "width": "15%" },
//            { data: 'jerseyNumber', "width": "10%" },
//            { data: 'dateOfBirth', "width": "15%" },
//            { data: 'teamId', "width": "10%" },
//            {
//                data: 'playerId',
//                "render": function (data) {
//                    return `<div class="w-75 btn-group" role="group">
//                     <a href="/TeamManager/Player/Upsert?playerId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
//                     <a onClick=Delete('/TeamManager/product/delete/${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
//                    </div>`
//                },
//                "width": "25%"
//            }
//        ]
//    });
//}


var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/TeamManager/Player/GetAll',
            data: { id: teamId } // ✅ now works
        },
        "columns": [
            { data: 'firstName', width: "25%" },
            { data: 'lastName', width: "15%" },
            { data: 'jerseyNumber', width: "10%" },
            {
                data: 'dateOfBirth',
                render: function (data) {
                    if (!data) return "";
                    return new Date(data).toLocaleDateString('en-GB');
                }
            },
            { data: 'teamId', width: "10%" },
            {
                data: 'playerId',
                render: function (data) {
                    return `
                    <div class="w-75 btn-group">
                        <a href="/TeamManager/Player/Upsert?playerId=${data}&id=${teamId}" class="btn btn-primary mx-2">
                            Edit
                        </a>
                        <a onClick=Delete('/TeamManager/Player/Delete/${data}') class="btn btn-danger mx-2">
                            Delete
                        </a>
                    </div>`;
                }
            }
        ]
    });
}