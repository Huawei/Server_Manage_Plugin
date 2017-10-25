# 华为服务器 Cacti 监控插件功能使用手册

------
## 一、说明
成功安装Cacti Huawer Server 插件后（详情请参考安装部署说明），点击导航栏上方 Huawei Server 进入插件页面，在这里查看对应的华为服务器列表，以及服务器对应的 iBMC、HMM、内存、CPU、硬盘等相关信息，同时也可以扫描或通过Excel模板批量导入服务器。

## 二、服务器导入
华为服务器 Cacti 插件安装后会自动创建两个 Host Template, 以及关联的多个Graph Template、Data Template、Data Query，您无需自己创建，只需在添加设备时指定相应的模板即可，服务器导入有以下几种途径：

 1. 手工添加服务器，参考下面第三章。
 2. 通过 Excel模板填写相应的服务器信息，并上传批量导入，请参考第四章。
 3. 通过扫描 IP 地址段，获取服务器列表，并自动导入，请参考第五章。
 
## 三、手工添加服务器

1. Cacti 左边导航菜单选择 Devices
![image_1bftipcn2f7p10ul1d9fecs15glm.png-202.4kB][1]

2. 右上角点 “Add” 进入创建新设备界面
- 填写服务器 IP 地址，SNMP连接认证等信息。
- Host Template 选择对应的服务器类型，机架服务器（Huawei Rack Server），或刀片框服务器（Blade Server）。
- 勾选“This is Huawei server with iBMC or HMM installed”
![image_1bftiscvd1mgg5t719i6lc18l13.png-273.7kB][2]
 
3. 点击右下角“Create”按钮保存。

4. 系统提示保存成功，此时并不会创建性能监控图表，点击右上角“Create Graphs for this Host”
![image_1bftj32d5ul3imf9jbichrts1g.png-117.9kB][3]

    对于机架服务器，您将看到3个Graph Template:
    > Huawei - iBMC CPU Usage
    > Huawei - iBMC Power Consumption
    > Huawei - iBMC Present System Power
    
    以及1个Data Query：
    > Huawei - iBMC Temperature Reading

    对于刀片机框，您将看到2个Graph Template:
    > Huawei - HMM Present System Power
    > Huawei - HMM Temperature Readin
    
    以及2个Data Query：
    > Huawei - HMM Blade CPU Usage
    > Huawei - HMM Blade Temperature

    - 点选相应的复选框全部选中
    - 如果Data Query 中看不到对应的实例，点击绿色小圆圈按钮刷新即可
    - 点击“Create”按钮保存。

 ![image_1bftjbpme80ql1r1sgu1i7n124b2a.png-229.6kB][4]

5. 系统提示相关的图表创建成功，点击上方导航栏Graphics按钮查看图表
![image_1bftjgph6riu12ek198c1m8hai82n.png-205.9kB][5]

6. 如果看不见，可在右上方切换显示模式，并选择相应的 Host 即可，图表创建好一般需要10分钟以上才能看见数据。
![image_1bftjk8o4189r14e5j2ccvib6434.png-472.4kB][6]

## 四、批量导入服务器
![image_1bf629vleak91ko8eeh12tg1cjm2n.png-309.3kB][7]

1. 点击 "Download Batch Import Template" 下载批量导入模板，填写服务器IP地址、SNMP连接等相关信息。

    模板中有以下字段需要填写：
    
    > ip - 服务器 IP 地址，如：192.168.0.10
    > snmp_ver - SNMP 版本，1.0版本填写1, 2.0和3.0版本则对应填写2,3
    > community - 团体名，SNMP 版本3.0无需填写，默认是 public
    
    以下字段仅在 SNMP 版本3.0时需要填写：
    
    > user_name - 填写用户名
    > sec_level - 安全级别，填写authNoPriv（认证但是不加密），或 authPriv（既认证又加密）
    > auth_protocol - 用户认证协议，填写 MD5 或 SHA
    > auth_passphrase - 填写用户密码
    > priv_protocol - 加密协议，填写 AES 或 DES，如果 sec_level 是 authNoPriv 不用填写
    > priv_passphrase - 加密密码，如果 sec_level 是 authNoPriv 不用填写

2. 点击"选择文件"按钮选择填好的模板文件，系统会自动上载并解析其中的服务器信息。

3. 选中 "Overwrite exist item(s)" 用以覆盖已经存在的服务器条目，否则如果指定服务器IP地址已经存在，将自动跳过不再导入。

4. 解析成功后，下方将显示出所有的服务器列表，如果需要，可人工修改 SNMP连接信息。

5. 点选右侧的复选框选中需要导入的服务器。

6. 点击 "Import Selected Servers" 开始导入，系统会显示进度条提示当前导入进度。

7. 导入完成后，会自动进入导入结果页面，如果导入失败，会显示失败的原因，如下图所示：

![image_1bf62qr2q1je8tcb557cdsabs34.png-200.1kB][9]

## 五、服务器自动扫描

![image_1bf62umrp17eaqr51tfuhd67n3h.png-328.6kB][10]

1. 输入扫描起始IP地址段，需要在一个网段内。

2. 点击 "Start Scan" 开始扫描，系统会自动显示扫描进度。

3. 选中 "Overwrite exist item(s)" 用以覆盖已经存在的服务器条目，否则如果指定服务器IP地址已经存在，将自动跳过不再导入。

4. 扫描完成，下方将显示出所有的服务器列表，对于ping超时的服务器，自动显示为红色背景。

5. 点选右侧的复选框选中需要导入的服务器，并输入 SNMP连接相关的信息。

6. 点击 "Import Selected Servers" 开始导入，系统会显示进度条提示当前导入进度，导入完成后将自动转到导入结果页面，此过程与Excel模板批量导入一致。

## 六、服务器列表页面
![image_1bf5vj26j1ngf411msni3261q9.png-239.4kB][11]

1. 华为Cacti插件的首页，上方为搜索过滤，可选择服务器状态，或输入服务器名称进行过滤，默认每页显示30条记录（可在Cacti Settings页面中修改），点"Go"提交设置，点"Clear"按钮清除过滤设置。

2. 默认显示机架服务器列表（Rack Servers），可点击相应Tab切换到刀片机框服务器列表（Blade Servers）。

3. 对于机架服务器，列表中会显示服务器名称（Host Name）、IP 地址、产品型号、iBMC版本、状态、CPU数量、内存硬盘大小等信息；对于机框服务器，显示服务器名称、IP地址、产品型号、SMM版本、刀片数等信息。

4. 直接点击服务器名称可查看该服务器的所有详细信息；或选择服务器（可选择最多10个），右下角选择对应的操作列表（查看内存信息等），点击 "Go"，查看服务器分类信息。

5. 右上角 "Batch Import Servers" 和 "Auto Scan Servers" 链接分别用以Excel批量导入服务器和通过IP地址段批量扫描服务器。

## 七、服务器详细信息
![image_1bf60c78j115l1bn4jg51qb372fm.png-252.2kB][12]

1. 从服务器列表页面点击服务器名称进入服务详细列表页面，上方显示服务器状态、型号、GUID、CPLD、UBOOT版本信息等基础信息。

2. 下方显示服务器所有组件的信息，相关组件如果有多个槽位会分别展示。

3. 点击右侧Refresh连接可通过SNMP连接服务器重新获取相关信息。

4. 对于机框服务器，还将显示刀片机列表，点击相应的刀片机名称可查看对应刀片机的详细信息，操作方式基本和查看服务器信息一致，如下图：

![image_1bf61759g1s2f13jupuvsq3i3r1t.png-62.7kB][13]

![image_1bf60vq709jhnj55rm140m8f31g.png-88.4kB][14]

## 八、服务器分类信息

![image_1bf61htmrkdj1iuipqb14e1abh2a.png-216kB][15]

1. 由服务器列表页面操作列表中可选择查看服务器分类信息，如果选择了多个服务器，每个服务器会以单独的TAB进行显示，可点击切换。

2. 下方显示具体的分类信息，相关组件如果有多个槽位会分别展示。

3. 点击右侧Refresh连接可通过SNMP连接服务器重新获取相关信息。

## 九、服务器性能图表
插件默认创建了以下图表：

1. 机架服务器

- CPU 占有率
- 服务器总功耗
- 服务器当前功耗
- 进风口温度

2. 刀片机框

- 单板CPU利用率
- 整框功耗
- 整框温度（Innet、Outlet、Ambient、LSW）
- 单板进风口温度

![image_1bftkdquq1k2b4nbpj895818e03h.png-623.8kB][16]


  [1]: http://static.zybuluo.com/jhzzzz/1ch0pjvqmyhbd0pxm9dtkq8f/image_1bftipcn2f7p10ul1d9fecs15glm.png
  [2]: http://static.zybuluo.com/jhzzzz/e1tf7khldg6lugj6wupu75du/image_1bftiscvd1mgg5t719i6lc18l13.png
  [3]: http://static.zybuluo.com/jhzzzz/mo3tjz8nul9xssunrzn0ooqg/image_1bftj32d5ul3imf9jbichrts1g.png
  [4]: http://static.zybuluo.com/jhzzzz/x7qy34d2sl577usv3hjoy7gh/image_1bftjbpme80ql1r1sgu1i7n124b2a.png
  [5]: http://static.zybuluo.com/jhzzzz/hev6w38yyu5j6mtqqtafd1fs/image_1bftjgph6riu12ek198c1m8hai82n.png
  [6]: http://static.zybuluo.com/jhzzzz/uz9mynr2udngl4sngtun5e10/image_1bftjk8o4189r14e5j2ccvib6434.png
  [7]: http://static.zybuluo.com/jhzzzz/4417wcdoo4kaywke5qoc30dz/image_1bf629vleak91ko8eeh12tg1cjm2n.png
  [8]: http://static.zybuluo.com/jhzzzz/l6mmpfa10o69xwdwg9gz7owq/image.png
  [9]: http://static.zybuluo.com/jhzzzz/6bvbpefxgd57l9glbwar6hxx/image_1bf62qr2q1je8tcb557cdsabs34.png
  [10]: http://static.zybuluo.com/jhzzzz/g78nfq9yjwt3njloeahvy1ud/image_1bf62umrp17eaqr51tfuhd67n3h.png
  [11]: http://static.zybuluo.com/jhzzzz/r9c9jg52os293e1u7mxh89uv/image_1bf5vj26j1ngf411msni3261q9.png
  [12]: http://static.zybuluo.com/jhzzzz/pxy707e8huiy7cix2x29q7sb/image_1bf60c78j115l1bn4jg51qb372fm.png
  [13]: http://static.zybuluo.com/jhzzzz/h0rjwhsaqvwvnkq5dcu7jw4n/image_1bf61759g1s2f13jupuvsq3i3r1t.png
  [14]: http://static.zybuluo.com/jhzzzz/s08082gmu3hreooxbx1xka2y/image_1bf60vq709jhnj55rm140m8f31g.png
  [15]: http://static.zybuluo.com/jhzzzz/zz7rc5gs8kc7mjt2pu90lbe6/image_1bf61htmrkdj1iuipqb14e1abh2a.png
  [16]: http://static.zybuluo.com/jhzzzz/clu6l2n3yrfuegvnlp1zxtzd/image_1bftkdquq1k2b4nbpj895818e03h.png
