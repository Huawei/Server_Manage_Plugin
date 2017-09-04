# -*- coding=utf-8 -*-
'''
###########################################################################
# Author:    kingkong
# Date:        May 6th 2017
# Modified:
#
# use example :
# mibevent2object for link trap node with event
#
###########################################################################
'''
from xml.etree.ElementTree import ElementTree, Element


OBJARRAY = []
DESCARRAY = []
SEVERITYARRAY = []
OIDARRAY = []


def changemibevent(objarray, descarray, sevarray,
                   oidarray, tree, nodes, devicetype):
    '''
    change mib trap name
    from BMC Mib hwCPUCATError to hwCPUCATErrorBMC
    from HMM Mib hwCPUCATError to hwCPUCATErrorHMM
    '''
    # B. 通过属性准确定位子节点
    nodenews = find_nodes(tree,
                          'object/tomanycont/object')

    for node in nodenews:
        if node.find('property[@id="oid"]') is None:
            continue
        oidstr = node.find('property[@id="oid"]').text
        if '1.3.6.1.4.1.2011.2.235.1.1.500.10' in oidstr:
            node.set('id', node.get('id')+devicetype)
            print node.get('id'), oidstr
        if '1.3.6.1.4.1.2011.2.235.1.1.500.11' in oidstr:
            node.set('id', node.get('id')+devicetype)
            print node.get('id'), oidstr
        if '1.3.6.1.4.1.2011.2.82.1.82.500.10' in oidstr:
            node.set('id', node.get('id')+'HMM')
            print node.get('id'), oidstr

def makedeviceevenmap(objarray, descarray, sevarray,
                      oidarray, tree, nodes, devicetype):
    '''
    create device event map
    '''
    # B. 通过属性准确定位子节点
    result_nodes = get_node_by_keyvalue(nodes,
                                        {"id":
                                         "/zport/dmd/Events/Huawei/" +
                                         devicetype})

    subnodes = find_nodes(result_nodes[0], "tomanycont")
    print result_nodes
    for idx in range(0, len(oidarray)):
        eventclasskey = objarray[idx]+devicetype
        desc = descarray[idx]
        oid = oidarray[idx]
        sev = sevarray[idx]
        print eventclasskey, '|', desc, '|', oid, '|', sev
        nodenews = find_nodes(tree,
                              'object/object[id="/zport/dmd/Events/Huawei/' + devicetype + '"]/tomanycont/object')
        nodeadds = get_node_by_keyvalue(nodenews,
                                        {"id": eventclasskey})
        # fixed event cancel things
        if 'Deassert' in eventclasskey:
            sev = '0'
        if len(nodeadds) == 0:
            addeventmapnode(tree, subnodes, eventclasskey, desc, oid, sev, devicetype)
        else:
            modifyeventmapnode(tree, subnodes, eventclasskey, desc, oid, sev, devicetype)


def addeventmapnode(tree, eventclassnodes, eventclasskey, desc, oid, sev, devicetype):
    '''
    add device event node
    '''
    padlinebreak = '\n'
    property1 = create_node(
        "property", {"type": "text", "id": "transform", "mode": "w"},
        'evt.summary = getattr(evt, "' + oid + '",'
        ' "' +
        desc + '");evt.severity=' + sev + ";" +
        "evt.component=getattr(evt,'hwTrapSensorName','');" +
        padlinebreak)

    property2 = create_node(
        "property", {"type": "string", "id": "eventClassKey", "mode": "w"},
        eventclasskey + padlinebreak)
    property3 = create_node(
        "property", {"type": "text", "id": "sequence", "mode": "w"},
        '7' + padlinebreak)
    evtmapnode = create_node(
        "object", {"id": eventclasskey,
                   "module": "Products.ZenEvents.EventClassInst",
                   "class": "EventClassInst"}, padlinebreak)

    add_child_node(eventclassnodes, evtmapnode)
    nodenews = find_nodes(tree, 'object/object[@id="/zport/dmd/Events/Huawei/' + devicetype + '"]/tomanycont/object')
    nodeadds = get_node_by_keyvalue(nodenews,
                                    {"id": eventclasskey})
    add_child_node(nodeadds, property1)
    add_child_node(nodeadds, property2)
    add_child_node(nodeadds, property3)


def modifyeventmapnode(tree, eventclassnodes, eventclasskey, desc, oid, sev, devicetype):
    '''
    modify device event node
    '''
    padlinebreak = '\n'

    parent_nodes = find_nodes(tree,
                              'object/object[@id="/zport/dmd/Events/Huawei/' + devicetype + '"]/tomanycont/object')

    del_parent_nodes = get_node_by_keyvalue(parent_nodes,
                                            {"id": eventclasskey})

    change_childnode_properties(del_parent_nodes, "property",
                                {"id": 'transform',
                                 'text': 'evt.summary = getattr(evt, "' +
                                 oid + '", "' + desc +
                                 '");evt.severity=' + sev +
                                 ";" + "evt.component = " +
                                 "getattr(evt, 'hwTrapSensorName', '');" +
                                 padlinebreak})

    # evt.component = getattr(evt, 'hwTrapSensorName', '');
    change_childnode_text(del_parent_nodes, "property",
                          {"id": 'transform'}, 'evt.summary = getattr(evt, "' +
                          oid + '", "' + desc + '");evt.severity=' +
                          sev + ";" + "evt.component = " +
                          "getattr(evt, 'hwTrapSensorName', '');" +
                          padlinebreak)

    change_childnode_text(del_parent_nodes, "property",
                          {"id": 'eventClassKey'},
                          eventclasskey + padlinebreak)

    change_childnode_text(del_parent_nodes, "property",
                          {"id": 'sequence'}, '7')

    # 准确定位子节点并删除之
#     delproperty1 = del_node_by_tagkeyvalue(
#         del_parent_nodes, "property", {"id": 'transform'})
#     delproperty2 = del_node_by_tagkeyvalue(
#         del_parent_nodes, "property", {"id": 'eventClassKey'})
#     delproperty3 = del_node_by_tagkeyvalue(
#         del_parent_nodes, "property", {"id": 'sequence'})


def readmibtraps(devicetype):
    '''
    read mib traps
    '''

    # txt文件和当前脚本在同一目录下，所以不用写具体路径
    filename = devicetype + 'mibtraps.txt'
    with open(filename, 'r') as file_to_read:
        while True:
            lines = file_to_read.readline()  # 整行读取数据
            if not lines:
                break

            # 将整行数据分割处理，如果分割符是空格，括号里就不用传入参数，如果是逗号， 则传入‘，'字符。
            obj, desc, severity, oid, = [str(i) for i in lines.split('\t')]
            OBJARRAY.append(obj)  # 添加新读取的数据
            DESCARRAY.append(desc)
            SEVERITYARRAY.append(severity)
            OIDARRAY.append(oid.strip('\n'))
#            print obj, desc, severity, oid
#         pos = np.array(pos)  # 将数据从list类型转换为array类型。
#         Efield = np.array(Efield)

#     print OBJARRAY, DESCARRAY, SEVERITYARRAY, OIDARRAY


# elemnt为传进来的Elment类，参数indent用于缩进，newline用于换行
def prettyxml(element, indent, newline, level=0):
    '''
    pretty xml
    '''
    if element is None:  # 判断element是否有子元素
        # 如果element的text没有内容
        if element.text is None or element.text.isspace():
            element.text = newline + indent * (level + 1)
        else:
            element.text = newline + indent * \
                (level + 1) + element.text.strip() + \
                newline + indent * (level + 1)
        # else:  # 此处两行如果把注释去掉，Element的text也会另起一行
        # element.text = newline + indent * (level + 1)
        # + element.text.strip() + newline + indent * level
    temp = list(element)  # 将elemnt转成list
    for subelement in temp:
        # 如果不是list的最后一个元素，说明下一个行是同级别元素的起始，缩进应一致
        if temp.index(subelement) < (len(temp) - 1):
            subelement.tail = newline + indent * (level + 1)
        else:  # 如果是list的最后一个元素， 说明下一行是母元素的结束，缩进应该少一个
            subelement.tail = newline + indent * level
        prettyxml(subelement, indent, newline, level=level + 1)  # 对子元素进行递归操作


def read_xml(in_path):
    '''读取并解析xml文件
        in_path: xml路径
        return: ElementTree'''
    tree = ElementTree()
    tree.parse(in_path)
    return tree


def write_xml(tree, out_path):
    '''将xml文件写出
        tree: xml树
        out_path: 写出路径'''
    tree.write(out_path, encoding="utf-8", xml_declaration=True)


def if_match(node, kv_map):
    '''判断某个节点是否包含所有传入参数属性
        node: 节点
        kv_map: 属性及属性值组成的map'''
    for key in kv_map:
        if node.get(key) != kv_map.get(key):
            return False
    return True


def find_nodes(tree, path):
    '''查找某个路径匹配的所有节点
        tree: xml树
        path: 节点路径'''
    return tree.findall(path)


def get_node_by_keyvalue(nodelist, kv_map):
    '''根据属性及属性值定位符合的节点，返回节点
        nodelist: 节点列表
        kv_map: 匹配属性及属性值map'''
    result_nodes = []
    for node in nodelist:
        if if_match(node, kv_map):
            result_nodes.append(node)
    return result_nodes


def change_node_properties(nodelist, kv_map, is_delete=False):
    '''修改/增加 /删除 节点的属性及属性值
        nodelist: 节点列表
        kv_map:属性及属性值map'''
    for node in nodelist:
        for key in kv_map:
            if is_delete:
                if key in node.attrib:
                    del node.attrib[key]
            else:
                node.set(key, kv_map.get(key))


def change_childnode_properties(nodelist, tag, kv_map, is_delete=False):
    '''修改/增加 /删除 节点的属性及属性值
        nodelist: 节点列表
        kv_map:属性及属性值map'''
    for parent_node in nodelist:
        children = parent_node.getchildren()
        for child in children:
            if child.tag == tag and if_match(child, kv_map):
                for key in kv_map:
                    if is_delete:
                        if key in child.attrib:
                            del child.attrib[key]
                    else:
                        child.set(key, kv_map.get(key))


def change_childnode_text(nodelist, tag, kv_map, text, is_add=False):
    '''改变/增加/删除一个节点的文本
        nodelist:节点列表
        text : 更新后的文本'''
    for parent_node in nodelist:
        children = parent_node.getchildren()
        for child in children:
            if child.tag == tag and if_match(child, kv_map):
                if is_add:
                    child.text += text
                else:
                    child.text = text


def change_node_text(nodelist, text, is_add=False, is_delete=False):
    '''改变/增加/删除一个节点的文本
        nodelist:节点列表
        text : 更新后的文本'''
    for node in nodelist:
        if is_add:
            node.text += text
        elif is_delete:
            node.text = ""
        else:
            node.text = text


def create_node(tag, property_map, content):
    '''新造一个节点
        tag:节点标签
        property_map:属性及属性值map
        content: 节点闭合标签里的文本内容
        return 新节点'''
    element = Element(tag, property_map)
    element.text = content
    return element


def add_child_node(nodelist, element):
    '''给一个节点添加子节点
        nodelist: 节点列表
        element: 子节点'''
    for node in nodelist:
        node.append(element)


def del_node_by_tagkeyvalue(nodelist, tag, kv_map):
    '''同过属性及属性值定位一个节点，并删除之
        nodelist: 父节点列表
        tag:子节点标签
        kv_map: 属性及属性值列表'''
    for parent_node in nodelist:
        children = parent_node.getchildren()
        for child in children:
            if child.tag == tag and if_match(child, kv_map):
                parent_node.remove(child)

if __name__ == "__main__":
    devicetype = 'BMC'

    # 1. 读取xml文件
    tree = read_xml("./objects.xml")

    # 2. 属性修改
    # A. 找到父节点
    nodes = find_nodes(tree,
                       'object/object')

    changemibevent(OBJARRAY, DESCARRAY, SEVERITYARRAY,
                   OIDARRAY, tree, nodes, devicetype)

    # 6. 输出到结果文件
    write_xml(tree, "./tmp1.xml")

    # 0. 读取txt文件
    readmibtraps(devicetype)

    # 1. 读取xml文件
    tree = read_xml("./tmp1.xml")

    # 2. 属性修改
    # A. 找到父节点
    nodes = find_nodes(tree,
                       'object/object')

    makedeviceevenmap(OBJARRAY, DESCARRAY, SEVERITYARRAY,
                      OIDARRAY, tree, nodes, devicetype)

    # 6. 输出到结果文件
    write_xml(tree, "./tmp2.xml")

    OBJARRAY = []
    DESCARRAY = []
    SEVERITYARRAY = []
    OIDARRAY = []

    devicetype = 'HMM'

    # 0. 读取txt文件
    readmibtraps(devicetype)

    # 1. 读取xml文件
    tree = read_xml("./tmp2.xml")

    # 2. 属性修改
    # A. 找到父节点
    nodes = find_nodes(tree, "object/object")

    makedeviceevenmap(OBJARRAY, DESCARRAY, SEVERITYARRAY,
                      OIDARRAY, tree, nodes, devicetype)

    # 6. 输出到结果文件
    write_xml(tree, "./out.xml")
