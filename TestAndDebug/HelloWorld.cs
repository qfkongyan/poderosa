﻿// Copyright 2004-2017 The Poderosa Project.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#define USING_GENERIC_COMMAND 

#if MONOLITHIC
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Document;
using Poderosa.View;
using Poderosa.Forms;
using Poderosa.Plugins;
using Poderosa.Commands;
using Poderosa.Terminal;

[assembly: PluginDeclaration(typeof(Poderosa.HelloWorldPlugin))]

namespace Poderosa {
    [PluginInfo(ID="org.poderosa.helloworld", Version=VersionInfo.PODEROSA_VERSION, Author=VersionInfo.PROJECT_NAME, Dependencies="org.poderosa.core.window")]
    internal class HelloWorldPlugin : PluginBase {
        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);

#if USING_GENERIC_COMMAND
            //"ダイアログ"コマンドカテゴリを取得
            ICoreServices cs = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));
            ICommandCategory dialog = cs.CommandManager.CommandCategories.Dialogs;

            //コマンド作成
            GeneralCommandImpl cmd = new GeneralCommandImpl("org.poderosa.helloworld", "Hello World Command", dialog, delegate(ICommandTarget target) {
                //コマンドの実装
                //このコマンドはメインメニューから起動するので、CommandTargetからウィンドウが取得できるはず
                IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
                Debug.Assert(window!=null);
                MessageBox.Show(window.AsForm(), "Hello World", "HelloWorld Plugin");
                return CommandResult.Succeeded;
            });
            //コマンドマネージャへの登録
            cs.CommandManager.Register(cmd);

            //ヘルプメニューに登録
            IExtensionPoint helpmenu = poderosa.PluginManager.FindExtensionPoint("org.poderosa.menu.help");
            helpmenu.RegisterExtension(new PoderosaMenuGroupImpl(new PoderosaMenuItemImpl("org.poderosa.helloworld", "Hello World")));
#else //単なるIPoderosaCommand版
            //コマンド作成
            PoderosaCommandImpl cmd = new PoderosaCommandImpl(delegate(ICommandTarget target) {
                //コマンドの実装
                //このコマンドはメインメニューから起動するので、CommandTargetからウィンドウが取得できるはず
                IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
                Debug.Assert(window!=null);
                MessageBox.Show(window.AsForm(), "Hello World", "HelloWorld Plugin");
                return CommandResult.Succeeded;
            });

            //ヘルプメニューに登録
            IExtensionPoint helpmenu = poderosa.PluginManager.FindExtensionPoint("org.poderosa.menu.help");
            helpmenu.RegisterExtension(new PoderosaMenuGroupImpl(new PoderosaMenuItemImpl(cmd, "Hello World")));
#endif
        }
    }

}
#endif
