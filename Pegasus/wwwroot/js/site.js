﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    var currentSettings = {
        projectId: $("#projectId").val(),
        taskFilterId: $("#taskFilterId").val()
    }

    // disable the CreateTask button when showing All Projects
    var createButton = $("#createButton");
    var setCreateTaskButton = function () {
        if (currentSettings.projectId === "0") {
            $(createButton).addClass("disabled");
        } else {
            $(createButton).removeClass("disabled");
        }
    }

    // set initial Create button state
    $(function () {
        setCreateTaskButton();
    });

    // hide all sidebar icons, then show the selected sidebar icon
    var projectListItems = $(".project-list-item");
    var taskFilterList = $(".task-filter");
    $(function () {
        $(projectListItems).find("i").addClass("hide");
        $(projectListItems).filter(function () {
            return $(this).attr("value") === currentSettings.projectId;
        }).find("i").removeClass("hide");
        $(taskFilterList).find("i").addClass("hide");
        $(taskFilterList[currentSettings.taskFilterId]).find("i").removeClass("hide");
    });

    var updateList = function () {
        var options = {
            url: $(".project-list-form").attr("action"),
            type: $(".project-list-form").attr("method"),
            data: $(".project-list-form").serialize()
        };

        $.ajax(options).done(function (data) {
            var $target = $($(".project-list-form").attr("data-pgs-target"));
            $target.replaceWith(data);
        });
    }

    var sidebarAction = function (item, hiddenItem, itemList) {
        var selectedId = $(item).attr("value");
        $(hiddenItem).val(selectedId);
        $(itemList).find("i").addClass("hide");
        $(item).find("i").removeClass("hide");
        updateList();
        return selectedId;
    };

    $(".body-content").on("click", ".project-list-item", function () {
        $("#page").val(1); // reset display back to page 1
        currentSettings.projectId = sidebarAction($(this), $("#projectId"), $(projectListItems));
        // need to disable Create button if All projects selected
        setCreateTaskButton();
        return false;
    });

    $(".body-content").on("click", ".task-filter", function () {
        $("#page").val(1); // reset display back to page 1
        sidebarAction($(this), $("#taskFilterId"), $(taskFilterList));
        return false;
    });

    $(".body-content").on("click", ".pagination-list-item", function () {
        sidebarAction($(this), $("#page"), null);
        return false;
    });

    $(".body-content").on('click', ".comment-edit-button", function () {
        $(this).addClass("hide").siblings(".comment-cancel-button").removeClass("hide");
        var editSection = $(this).parents(".comment-edit-section");
        $(editSection).find(".task-comment").addClass("hide");
        $(editSection).find(".comment-task-edit").removeClass("hide");
        $(".comment-task-text").each(function () {
            $(this).css("height", "auto").css("height", this.scrollHeight + this.offsetHeight);
        });
    });
    $(".body-content").on('click', ".comment-cancel-button", function () {
        $(this).addClass("hide").siblings(".comment-edit-button").removeClass("hide");
        var editSection = $(this).parents(".comment-edit-section");
        var taskComment = $(editSection).find(".task-comment");
        $(taskComment).removeClass("hide").siblings(".comment-task-edit").addClass("hide")
            .find("textarea").val($(taskComment).find("p").html());
    });

    $(".body-content").on('click', ".comment-delete-button", function () {
        $(this).siblings("input").val(true);
        updateList();
    });

    $(".navbar-content").on("click", ".settings-link", function () {
        if ($(".settings-sidebar").hasClass("on")) {
            $(".settings-sidebar").removeClass("on");
        } else {
            $(".settings-sidebar").addClass("on");
        }
        return false;
    });
})