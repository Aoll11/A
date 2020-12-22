
    $(function () {

        /*Confirm page deletion*/
        $("a.delete").click(function () {
            if (!confirm("Confirm ")) return false /*по умолчанию confirm  false */
        });             /*Html.ActionLink в разметке преобразуется в тег  a , ищем тот, который с классом class = "delete", подступаемся к его методу click, на котором запускается анонимная фунция и в ней с помощью confirm запрашивается подтверждение и если нажата отмена, то возвращает false и тогда удаление не произойдёт */

            /*____________________________________________________________________________*/
            /*Sorting script */
            $("table#pages tbody").sortable({
        items: "tr:not(.home)",
                placeholder: "ui-state-highlight",
                update: function () {
                    var ids = $("table#pages tbody").sortable("serialize");
                    var url = "/Admin/Pages/ReorderPages";

                    $.post(url, ids, function (data) {});
                }
            });

        });
  