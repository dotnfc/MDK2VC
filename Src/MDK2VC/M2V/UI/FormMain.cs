﻿using MDK2VC.M2V;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace MDK2VC
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// 项目配置
        /// </summary>
        SysConfig cfg;
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            cfg = SysConfig.Current;
            tBoxMDKPath.Text = cfg.MdkPath;
            tBoxVCPath.Text = cfg.VcPath;
        }

        private void btnSelMDKPath_Click(object sender, EventArgs e)
        {
            var fileDlg = new OpenFileDialog();
            fileDlg.Multiselect = true;
            fileDlg.Title = "请选择文件";
            fileDlg.Filter = "MDK|*.uvprojx";
            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                tBoxMDKPath.Text = fileDlg.FileName;
                cfg.MdkPath = fileDlg.FileName;
                cfg.Save();
            }
        }

        private void btnSelectVCPath_Click(object sender, EventArgs e)
        {
            var fileDlg = new SaveFileDialog();
            fileDlg.Title = "请选择文件";
            fileDlg.Filter = "VC项目|*.vcxproj";
            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                tBoxVCPath.Text = fileDlg.FileName;
                cfg.VcPath = fileDlg.FileName;
                cfg.Save();
            }
        }

        private void btnTrans_Click(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            getDefine(builder);
            getIncludePath(builder);
            getGroups(builder);
            richTextBox1.Text = builder.ToString();
        }
        void getIncludePath(StringBuilder builder)
        {
            var doc = XElement.Load(cfg.MdkPath);
            var Targets = doc.Element("Targets");
            var Target = Targets.Element("Target");
            var TargetOption = Target.Element("TargetOption");
            var TargetArmAds = TargetOption.Element("TargetArmAds");
            var Cads = TargetArmAds.Element("Cads");
            var VariousControls = Cads.Element("VariousControls");
            var IncludePath = VariousControls.Element("IncludePath");
                                   
            builder.AppendLine(IncludePath.Value);
           
        }
        void getDefine(StringBuilder builder)
        {
            var doc = XElement.Load(cfg.MdkPath);
            var Targets = doc.Element("Targets");
            var Target = Targets.Element("Target");
            var TargetOption = Target.Element("TargetOption");
            var TargetArmAds = TargetOption.Element("TargetArmAds");
            var Cads = TargetArmAds.Element("Cads");
            var VariousControls = Cads.Element("VariousControls");
            var Define = VariousControls.Element("Define");
                        
            builder.AppendLine(Define.Value);
        }
        void getGroups(StringBuilder builder)
        {
            var doc = XElement.Load(cfg.MdkPath);
            var Targets = doc.Element("Targets");
            var Target = Targets.Element("Target");
            var Groups = Target.Element("Groups");

            var Group = Groups.Elements("Group");
            foreach(var grou in Group)
            {                
                var aa = grou.Element("GroupName");
                builder.AppendLine(aa.Value);
                var Files = grou.Elements("Files");
                foreach (var File in Files)
                {
                    var file = File.Elements("File");
                    foreach (var ff in file)
                    {
                        var FilePath = ff.Element("FilePath");
                        if (FilePath != null)
                            builder.AppendLine(FilePath.Value);
                    }
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var builderdef = new StringBuilder();
            getDefine(builderdef);

            var builderinclude = new StringBuilder();
            getIncludePath(builderinclude);

            var xDoc = new XDocument();
            var Project = new XElement("Project",new XAttribute("DefaultTargets", "Build"));
            var ItemGroup = new XElement("ItemGroup",new XAttribute("Label", "ProjectConfigurations"));

            var ItemDefinitionGroup = new XElement("ItemDefinitionGroup",new XAttribute("Condition", @"'$(Configuration)|$(Platform)'=='Release|x64'"));
            var ClCompile = new XElement("ClCompile");
            var AdditionalIncludeDirectories = new XElement("AdditionalIncludeDirectories", builderinclude.ToString());
            var PreprocessorDefinitions = new XElement("PreprocessorDefinitions", builderdef.ToString()+@";%(PreprocessorDefinitions)");


            ClCompile.Add(AdditionalIncludeDirectories);
            ClCompile.Add(PreprocessorDefinitions);

            ItemDefinitionGroup.Add(ClCompile);

            var ItemGroupfiles = new XElement("ItemGroup");
            for(int i=0;i<10;i++)
            {
                var f1 = new XElement("ClCompile" , new XAttribute("Include", @"..\..\STDOS\App\AT.cpp"));   
                
                ItemGroupfiles.Add(f1);
            }



            Project.Add(ItemGroup);
            Project.Add(ItemDefinitionGroup);
            Project.Add(ItemGroupfiles);


            xDoc.Add(Project);


            xDoc.Save(cfg.VcPath);
        }
    }
}
