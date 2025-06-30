const breadcrumb = document.getElementById('breadcrumb-container');

function updateBreadcrumb() {
    const path = window.location.pathname.split('/').filter(part => part);
    breadcrumbContainer.innerHTML = ''; // Clear existing breadcrumb
    breadcrumbContainer.innerHTML +='<li class="breadcrum-item"><a href="/"> Home </a> </li>';

    if (controllerName !== "Home") {
        breadcrumbContainer.innerHTML +=
            '<li class="breadcrum-item"><a href="/${controllerName}"> ${controllerName} </a> </li>';
    }
    breadcrumbContainer.innerHTML +=
        '<li class="breadcrum-item active" aria-current="page"> $(actionName)</li>';

}
updateBreadcrumb();
