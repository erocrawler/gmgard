(function ($) {
    var cate = $('#category-tree');
    if (cate.length == 0) { return; }
    var catej = cate.val();
    var categories = JSON.parse(catej);
    var $catval = $('#CategoryID');
    var $maincat = $('<select><option></option></select>');

    function AddOption($select, cates) {
        for (var i = 0; i < cates.length; i++) {
            var option = $('<option></option>');
            option.attr("value", cates[i].CategoryID);
            option.text(cates[i].CategoryName);
            option.data('index', i);
            $select.append(option);
        }
    }
    $catval.hide().before($maincat);
    function AddSelect($select, categories) {
        AddOption($select, categories);
        $select.change(function () {
            var pcateid = $select.val();
            var categoryIndex = $select.find(':selected').data('index');
            $select.nextAll('.subcat-select').remove();
            var selectCategory = categories[categoryIndex];
            $catval.val(selectCategory ? pcateid : 0);
            $catval.data('leaf', true);
            if (selectCategory && selectCategory.SubCategories.length > 0) {
                var $subselect = $('<select class="subcat-select"><option value="0"></option></select>');
                AddSelect($subselect, selectCategory.SubCategories);
                $select.after($subselect);
                $catval.data('leaf', false);
            }
        });
    }
    AddSelect($maincat, categories);

    function GetCategoryPathToID(path, cates, id) {
        for (var i = 0; i < cates.length; i++) {
            if (cates[i].CategoryID == id) {
                path.push(cates[i]);
                return true;
            }
            path.push(cates[i]);
            if (GetCategoryPathToID(path, cates[i].SubCategories, id)) {
                return true;
            }
            path.pop();
        }
        return false;
    }
    if (!$catval.val()) {
        $catval.val(window.localStorage["gmcategory"]);
    }
    if ($catval.val()) {
        var categoryPath = [];
        GetCategoryPathToID(categoryPath, categories, $catval.val());
        var $select = $maincat;
        for (var i = 0; i < categoryPath.length; ++i) {
            $select.val(categoryPath[i].CategoryID);
            $select.trigger('change');
            $select = $select.next('.subcat-select');
        }
    }
})(jQuery);