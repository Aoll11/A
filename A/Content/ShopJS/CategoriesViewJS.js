﻿
$(function () {

    /* Add new category
     * 9 *
     * Объявляем и инициализируем нужные переменные */

    var newCatA = $("a#newcata"); /*Класс линка добавления*/
    var newCatTextInput = $("#newcatname"); /*Класс текстового поля ввода*/
    var ajaxText = $("span.ajax-text"); /*Класс картинки загрузки*/
    var table = $("table#pages tbody"); /*Класс таблицы вывода*/

    /* Пишем функцию на отлов нажатия Enter */
    newCatTextInput.keyup(function (e) {
        if (e.keyCode == 13) { /* 13 кон "интер", отлавливает нажатие на "ввод" */
            newCatA.click();
        }
    });

    /* Пишем функцию Click */
    newCatA.click(function (e) {
        e.preventDefault();

        var catName = newCatTextInput.val();

        if (catName.length < 3) {
            alert("Category name must be at least 3 characters long.");
            return false;
        }

        ajaxText.show();

        var url = "/admin/shop/AddNewCategory";

        $.post(url, { catName: catName }, function (data) {
            var response = data.trim();

            if (response == "titletaken") {
                ajaxText.html("<span class='alert alert-danger'>That title is taken!</span>");
                setTimeout(function () {
                    ajaxText.fadeOut("fast", function () {
                        ajaxText.html("<img src='/Content/img/ajax-loader.gif' height='50' />");
                    });
                }, 2000);
                return false;
            }
            else {
                if (!$("table#pages").length) {
                    location.reload();
                }
                else {
                    ajaxText.html("<span class='alert alert-success'>The category has been added!</span>");
                    setTimeout(function () {
                        ajaxText.fadeOut("fast", function () {
                            ajaxText.html("<img src='/Content/img/ajax-loader.gif' height='50' />");
                        });
                        window.location.reload(); }, 2000); /*   window.location.reload();  - добавлено  */

                    newCatTextInput.val("");

                    var toAppend = $("table#pages tbody tr:last").clone();
                    toAppend.attr("id", "id_" + data);
                    toAppend.find("#item_Name").val(catName);
                    toAppend.find("a.delete").attr("href", "/admin/shop/DeleteCategory/" + data);
                    table.append(toAppend);
                    table.sortable("refresh");
                }
            }
        });
    });

    /*____________________________________________________________________________*/


    /*Confirm category deletion*/

    $("body").on("click", "a.delete", function () {
        if (!confirm("Confirm category deletion ")) return false /*по умолчанию confirm  false */
    });             /*Html.ActionLink в разметке преобразуется в тег  a , ищем тот, который с классом class = "delete", подступаемся к его методу click, на котором запускается анонимная фунция и в ней с помощью confirm запрашивается подтверждение и если нажата отмена, то возвращает false и тогда удаление не произойдёт */

    /*____________________________________________________________________________*/


    /* Rename category script*/

    var originalTextBoxValue;

    $("table#pages input.text-box").dblclick(function () {
        originalTextBoxValue = $(this).val();
        $(this).attr("readonly", false);
    });

    $("table#pages input.text-box").keyup(function (e) {
        if (e.keyCode == 13) {
            $(this).blur();
        }
    });

    $("table#pages input.text-box").blur(function () {
        var $this = $(this);
        var ajaxdiv = $this.parent().parent().parent().find(".ajaxdivtd");
        var newCatName = $this.val();
        var id = $this.parent().parent().parent().parent().parent().attr("id").substring(3);
        var url = "/admin/shop/RenameCategory";

        if (newCatName.length < 3) {
            alert("Category name must be at least 3 characters long.");
            $this.attr("readonly", true);
            return false;
        }

        $.post(url, { newCatName: newCatName, id: id }, function (data) {
            var response = data.trim();

            if (response == "titletaken") {
                $this.val(originalTextBoxValue);
                ajaxdiv.html("<div class='alert alert-danger'>That title is taken!</div>").show();
            }
            else {
                ajaxdiv.html("<div class='alert alert-success'>The category name has been changed!</div>").show();
            }

            setTimeout(function () {
                ajaxdiv.fadeOut("fast", function () {
                    ajaxdiv.html("");
                });
            }, 3000);
        }).done(function () {
            $this.attr("readonly", true);
        });
    });
    /*____________________________________________________________________________*/


    /*Sorting script */
    $("table#pages tbody").sortable({
        items: "tr:not(.home)",
        placeholder: "ui-state-highlight",
        update: function () {
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/Shop/ReorderCategories";

            $.post(url, ids, function (data) { });
        }
    });

});