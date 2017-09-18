var select_templateType = [{
    value: 'OS',
    label: 'OS模板'
}, {
    value: 'POWER',
    label: '上下电模板'
}];

/**
 * 获取操作栏添加和删除图标按钮
 * @templateType 模板类别
 */
function select_templateChange(templateType)
{
    switch (templateType) {
        case 'OS':
            window.location.href = 'addOS.html';
            break;
        case 'POWER':
            window.location.href = 'addPower.html';
            break;
        default:
            break;
    }
}