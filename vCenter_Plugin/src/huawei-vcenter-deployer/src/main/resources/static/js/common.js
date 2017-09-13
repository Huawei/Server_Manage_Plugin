/**
 * 改变当前语言
 * @param {string} lang (zhCN,en)
 */
function changelang(lang) {
    if (lang == 'zhCN') {
        ELEMENT.locale(ELEMENT.lang.zhCN);
        localStorage.setItem('lang', 'zhCN');
        this.lang = ELEMENT.lang.zhCN.el.templateManage;
    } else {
        ELEMENT.locale(ELEMENT.lang.en);
        this.lang = ELEMENT.lang.en.el.templateManage;
        localStorage.setItem('lang', 'en');
    }
}
/**
 * 国际化
 **/
function getIn18() {
    var lang = localStorage.getItem('lang');
    if (lang) {
        if (lang == 'en') {
            ELEMENT.locale(ELEMENT.lang.en);
            return i18n_en;
        } else {
            ELEMENT.locale(ELEMENT.lang.zhCN);
            return i18n_zh_CN;

        }
    } else {
        ELEMENT.locale(ELEMENT.lang.zhCN);
        return i18n_zh_CN;
    }
}