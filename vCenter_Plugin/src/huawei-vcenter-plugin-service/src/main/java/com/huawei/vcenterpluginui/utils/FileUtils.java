package com.huawei.vcenterpluginui.utils;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.attribute.AclEntry;
import java.nio.file.attribute.AclEntryPermission;
import java.nio.file.attribute.AclEntryType;
import java.nio.file.attribute.AclFileAttributeView;
import java.nio.file.attribute.PosixFilePermission;
import java.nio.file.attribute.UserPrincipal;
import java.util.EnumSet;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

public class FileUtils {
	private static final Log LOGGER = LogFactory.getLog(FileUtils.class);

	private static final String OS = System.getProperty("os.name").toLowerCase(Locale.US);

	private static final String VMWARE_LINUX_DIR = "/home/vsphere-client/base";

	private static final String VMWARE_WINDOWS_DIR = "C:/ProgramData/VMware/vCenterServer/runtime/base";

	public static final String BASE_FILE_NAME = "baseV3.txt";
	
	public static final String WORK_FILE_NAME = "workV3.txt";

	public static String getKey(String fileName) {
		File file = new File(getPath() + File.separator + fileName);
		createFile(file);
		String line = null;
		StringBuffer result = new StringBuffer();
		BufferedReader br = null;
		try {
			br = new BufferedReader(new InputStreamReader(new FileInputStream(file), "utf-8"));
			while ((line = br.readLine()) != null) {
				result.append(line);
			}
			if(result.length()<1){
				return null;
			}
			return result.toString();
		} catch (IOException e) {
			LOGGER.error(e.getMessage());
		} finally {
			if (br != null) {
				try {
					br.close();
				} catch (IOException e) {
					LOGGER.error(e.getMessage());
				}
			}
		}
		return null;
	}

	public static void saveKey(String key,String fileName) {
		File file = new File(getPath() + File.separator + fileName);
		createFile(file);
		BufferedWriter bw = null;
		try {
			bw = new BufferedWriter(new OutputStreamWriter(new FileOutputStream(file, false), "utf-8"));
			bw.write(key);
		} catch (IOException e) {
			LOGGER.error(e.getMessage() + " svae key have an error:" + key);
		} finally {
			if (bw != null) {
				try {
					bw.close();
				} catch (IOException e) {
					LOGGER.error(e.getMessage());
				}
			}
		}
	}

	private static void createFile(File file) {
		// 判断文件是否存在
		if (!file.exists()) {
			LOGGER.info("key file not exists, create it ...");
			try {
				createDir(getPath());
				boolean re = file.createNewFile();
				if (re) {
					//设置权限
					setFilePermission(file);
				} else {
					LOGGER.info("create file failed");
				}
			} catch (IOException e) {
				LOGGER.error(e.getMessage());
			}
		}
	}
	
	private static void setFilePermission(File file) throws IOException {
		if(isWindows()){
			setWindowsFilePermission(file);
		}else{
			setLinuxFilePermission(file);
		}
	}
	
	public static void setWindowsFilePermission(File file) throws IOException {
		Path path = Paths.get(file.getAbsolutePath());

		// Read Acl
	      AclFileAttributeView view = Files.getFileAttributeView(path, AclFileAttributeView.class);
	      List<AclEntry> acl = view.getAcl();
	      for (AclEntry ace : acl) {
	    	LOGGER.info("Ace Type: " + ace.type().name() + ace.principal().getName());
	    	StringBuffer permsStr = new StringBuffer();
	        for (AclEntryPermission perm : ace.permissions()) {
	          permsStr.append(perm.name() + " ");
	        }
	        LOGGER.info("Ace Permissions: " + permsStr.toString().trim());
	      }
	      acl.clear();
	      // Add Acl
	      // Get user
	      UserPrincipal user = path.getFileSystem().getUserPrincipalLookupService().lookupPrincipalByName(System.getProperty("user.name"));
	      AclEntry entry = AclEntry.newBuilder().setPermissions(EnumSet.of(
	          AclEntryPermission.READ_NAMED_ATTRS,
	          AclEntryPermission.WRITE_NAMED_ATTRS,
	          AclEntryPermission.APPEND_DATA,
	          AclEntryPermission.READ_ACL,
	          AclEntryPermission.WRITE_OWNER,
	          AclEntryPermission.DELETE_CHILD,
	          AclEntryPermission.SYNCHRONIZE,
	          AclEntryPermission.WRITE_DATA,
	          AclEntryPermission.WRITE_ATTRIBUTES,
	          AclEntryPermission.READ_DATA,
	          AclEntryPermission.DELETE,
	          AclEntryPermission.WRITE_ACL,
	          AclEntryPermission.READ_ATTRIBUTES,
	          AclEntryPermission.EXECUTE
	      )).setType(AclEntryType.ALLOW).setPrincipal(user).build();
	      acl.add(0, entry); // insert before any DENY entries
	      view.setAcl(acl);
	}
	
	public static void setLinuxFilePermission(File file) throws IOException {
		Set<PosixFilePermission> perms = new HashSet<PosixFilePermission>();
	    perms.add(PosixFilePermission.OWNER_READ);
	    perms.add(PosixFilePermission.OWNER_WRITE);
	    perms.add(PosixFilePermission.GROUP_READ);
	    perms.add(PosixFilePermission.GROUP_WRITE);
	    try {
	        Path path = Paths.get(file.getAbsolutePath());
	        Files.setPosixFilePermissions(path, perms);
	    } catch (Exception e) {
	    	LOGGER.error("Change folder " + file.getAbsolutePath() + " permission failed.", e);
	    }
	}

	private static Boolean isWindows() {
		return OS.indexOf("windows") >= 0;
	}

	public static String getPath() {
		if (isWindows()) {
			return VMWARE_WINDOWS_DIR;
		} else {
			return VMWARE_LINUX_DIR;
		}
	}

	// 创建目录
	public static boolean createDir(String destDirName) {
		File dir = new File(destDirName);
		if (dir.exists()) {// 判断目录是否存在
			LOGGER.info("创建目录失败，目标目录已存在！");
			return false;
		}
		if (!destDirName.endsWith(File.separator)) {// 结尾是否以"/"结束
			destDirName = destDirName + File.separator;
		}
		if (dir.mkdirs()) {// 创建目标目录
			LOGGER.info("创建目录成功！" + destDirName);
			return true;
		} else {
			LOGGER.error("创建目录失败！");
			return false;
		}
	}
}
