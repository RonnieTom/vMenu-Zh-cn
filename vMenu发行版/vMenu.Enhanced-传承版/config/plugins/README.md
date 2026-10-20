# 请勿删除此文件夹

插件带来的权限与设置模板会放在这里。插件是别人写的独立资源，会在 vMenu 内增加自己的菜单，因此其权限与设置不属于 vMenu 本身，也不应写进 vMenu 的 `permissions.cfg` 与 `configuration.cfg`。

vMenu 可以向此文件夹写入文件，但前提是文件夹存在。若你删掉它，vMenu 无法新建，所有插件的模板都不会出现。

## 这里会出现什么

每个注册成功的插件都会得到两个文件，均以其所在资源名命名：

```
<资源名>.permissions.cfg.example
<资源名>.configuration.cfg.example
```

例如资源名为 `vMenu.ExamplePlugin` 的插件会生成 `vMenu.ExamplePlugin.permissions.cfg.example` 与 `vMenu.ExamplePlugin.configuration.cfg.example`。

这些文件会在该插件每次注册时重写（也就是插件或 vMenu 每次启动时）。
插件未运行时不会写入任何内容，所以请先启动插件再来查看。
切勿直接编辑 `.example` 文件本身，下次启动时你的修改会被清空。

## 如何使用

用法与 vMenu 自带的两份配置相同。
复制文件，去掉副本名里的 `.example`，编辑副本，并在 `server.cfg` 中、启动 vMenu 及其插件的那一行之上 exec：

```
# 执行插件配置
exec @vMenu.Enhanced/config/plugins/vMenu.ExamplePlugin.permissions.cfg
exec @vMenu.Enhanced/config/plugins/vMenu.ExamplePlugin.configuration.cfg

# 执行 vMenu Enhanced 配置
exec @vMenu.Enhanced/config/permissions.cfg
exec @vMenu.Enhanced/config/configuration.cfg

# 允许 vMenu Enhanced 写入自己的文件夹
add_filesystem_permission vMenu.Enhanced write vMenu.Enhanced

# 启动 vMenu Enhanced 与插件，启动顺序无关
ensure vMenu.Enhanced
ensure vMenu.ExamplePlugin
```

建议把插件的配置与权限文件和主 vMenu 配置分开存放。
这样若要禁用某个插件，只需停止该插件并去掉对应的 `exec` 行即可。
若把权限与配置写进主配置，每次启用/禁用插件都要进去改一遍。

每个文件顶部都会说明自身用途，以及来自哪个插件。

## 移除插件

删除本文件夹中以该资源名开头的全部文件，并从 `server.cfg` 中去掉其 `exec` 行即可。vMenu 配置里不会再留下该插件的其他内容。
