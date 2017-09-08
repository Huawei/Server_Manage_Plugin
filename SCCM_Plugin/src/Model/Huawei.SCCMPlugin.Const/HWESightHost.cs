using System.Collections.Generic;

namespace Huawei.SCCMPlugin.Const
{
    public partial class ConstMgr
    {
        /// <summary>
        /// eSight����
        /// </summary>
        public class HWESightHost : ConstBase
        {
            private static HWESightHost _instance = null;
            /// <summary>
            /// eSight�������ϴ�����״��
            /// ״̬,NONE,ONLINE,DISCONNECT,FAILED
            /// </summary>
            public const string AS_LATEST_STATUS = "LATEST_STATUS";

            public const string LATEST_STATUS_NONE = "NONE";
            public const string LATEST_STATUS_ONLINE = "ONLINE";
            public const string LATEST_STATUS_DISCONNECT = "DISCONNECT";
            public const string LATEST_STATUS_FAILED = "FAILED";

            /// <summary>
            /// eSight������,http����Ĭ�ϳ�ʱʱ�䡣
            /// </summary>
            public const int DEFAULT_TIMEOUT_SEC = 30 * 60;
            /// <summary>
            /// http����URL��pattern
            /// </summary>
            public const string BASE_URI_FROMATE = "{0}://{1}:{2}/";
            /// <summary>
            /// ��½eSight REST URL
            /// </summary>
            public const string URL_LOGIN = "rest/openapi/sm/session";
            /// <summary>
            ///��������ѯ REST URL
            /// </summary
            public const string URL_DEVICEPAGE = "rest/openapi/server/device";
            /// <summary>
            ///��������ϸ��ѯ REST URL
            /// </summary
            public const string URL_DEVICEDETAIL = "rest/openapi/server/device/detail";

            /// <summary>
            ///�ϴ����Դ REST URL
            /// </summary
            public const string URL_UPLOADE_SOFTWARESOURCE = "rest/openapi/server/deploy/softwaresource/upload";
            /// <summary>
            ///�ϴ����Դ���Ȳ�ѯ REST URL
            /// </summary
            public const string URL_PROGRESS_SOFTWARESOURCE = "rest/openapi/server/deploy/softwaresource/progress";
            /// <summary>
            ///ɾ�����Դ REST URL
            /// </summary
            public const string URL_DELETE_SOFTWARESOURCE = "rest/openapi/server/deploy/softwaresource/delete";
            /// <summary>
            ///���Դ�б��ѯ REST URL
            /// </summary
            public const string URL_PAGE_SOFTWARESOURCE = "rest/openapi/server/deploy/software/list";

            /// <summary>
            ///����ģ���ѯ REST URL
            /// </summary
            public const string URL_PAGE_DEPLOY_TEMPLATE = "rest/openapi/server/deploy/template/list";
            /// <summary>
            ///��Ӳ���ģ�� REST URL
            /// </summary
            public const string URL_DEPLOY_TEMPLATE = "rest/openapi/server/deploy/template";
            /// <summary>
            ///ģ������ REST URL
            /// </summary
            public const string URL_DETAIL_TEMPLATE = "rest/openapi/server/deploy/template/detail";
            /// <summary>
            ///ɾ������ģ�� REST URL
            /// </summary
            public const string URL_DELETE_DEPLOYTEMPLATE = "rest/openapi/server/deploy/template/del";
            /// <summary>
            ///����ģ�嵽������ REST URL
            /// </summary
            public const string URL_TASK_DEPLOY = "rest/openapi/server/deploy/task";
            /// <summary>
            ///����ģ�嵽���������� REST URL
            /// </summary
            public const string URL_PROGRESS_DEPLOY = "rest/openapi/server/deploy/task/detail";
                        
            /// <summary>
            ///�̼��ϴ� REST URL
            /// </summary
            public const string URL_UPLOADE_BASEPACKAGE = "rest/openapi/server/firmware/basepackages/upload";
            /// <summary>
            ///�̼��ϴ����� REST URL
            /// </summary
            public const string URL_PROGRESS_BASEPACKAGE = "rest/openapi/server/firmware/basepackages/progress";
            /// <summary>
            /// �̼�-ɾ��������
            /// </summary>
            public const string URL_DELETE_BASEPACKAGE = "rest/openapi/server/firmware/basepackage/del";
            /// <summary>
            ///�̼��б��ѯ REST URL
            /// </summary
            public const string URL_PAGE_BASEPACKAGE = "rest/openapi/server/firmware/basepackages/list";
            /// <summary>
            ///�̼��б��ѯ-��ϸ REST URL
            /// </summary
            public const string URL_DETAIL_BASEPACKAGE = "rest/openapi/server/firmware/basepackage/detail";
 
            /// <summary>
            ///�̼������������� REST URL
            /// </summary                                        
            public const string URL_TASK_BASEPACKAGE = "rest/openapi/server/firmware/task";
            /// <summary>
            ///�̼�������������-���� REST URL
            /// </summary           
            public const string URL_TASK_PROGRESS_BASEPACKAGE = "rest/openapi/server/firmware/taskdetail";

            public const string URL_TASK_DN_PROGRESS_BASEPACKAGE = "rest/openapi/server/firmware/taskdevicedetail";

            public HWESightHost()
            {
                Dictionary<string, string> latestStatusDict = new Dictionary<string, string>();
                latestStatusDict.Add(LATEST_STATUS_NONE, "��ʼ");
                latestStatusDict.Add(LATEST_STATUS_ONLINE, "����");
                latestStatusDict.Add(LATEST_STATUS_DISCONNECT, "�Ͽ�");
                latestStatusDict.Add(LATEST_STATUS_FAILED, "����ʧ��");

                this.keyTable[AS_LATEST_STATUS] = latestStatusDict;
            }

            public static HWESightHost Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new HWESightHost();
                    }
                    return _instance;
                }
            }
        }
    }
}
