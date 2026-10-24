 
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
            { data: 'email', defaultContent: '', width: "20%" },
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