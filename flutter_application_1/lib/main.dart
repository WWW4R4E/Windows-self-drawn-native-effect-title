import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Demo',
      theme: ThemeData(primarySwatch: Colors.blue),
      home: const MyHomePage(),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key});

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  static const platform = MethodChannel('samples.flutter.dev/battery');
  String _batteryLevel = 'Unknown battery level.';

  Future<void> _getBatteryLevel() async {
    String batteryLevel;
    try {
      final result = await platform.invokeMethod<int>('getBatteryLevel');
      batteryLevel = 'Battery level at $result%.';
    } on PlatformException catch (e) {
      batteryLevel = "Failed to get battery level: '${e.message}'.";
    }

    setState(() {
      _batteryLevel = batteryLevel;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Battery Level'),
        actions: [
          // 最小化按钮
          TextButton(
            style: TextButton.styleFrom(
              foregroundColor: Colors.white,
              padding: const EdgeInsets.symmetric(horizontal: 16.0),
            ),
            onPressed: null,
            child: const Text('🗕', style: TextStyle(fontSize: 18)),
          ),

          // 最大化按钮 🗖
          TextButton(
            style: TextButton.styleFrom(
              foregroundColor: Colors.white,
              padding: const EdgeInsets.symmetric(horizontal: 16.0),
            ),
            onPressed: _getBatteryLevel,
            child: const Text('🗖', style: TextStyle(fontSize: 18)),
          ),

          // 关闭按钮 🗙
          TextButton(
            style: TextButton.styleFrom(
              foregroundColor: Colors.white,
              padding: const EdgeInsets.symmetric(horizontal: 16.0),
            ),
            onPressed: null,
            child: const Text('🗙', style: TextStyle(fontSize: 18)),
          ),
        ],
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: [
            ElevatedButton(
              onPressed: _getBatteryLevel,
              child: const Text('Get Battery Level'),
            ),
            Text(_batteryLevel),
          ],
        ),
      ),
    );
  }
}
// Get battery level.

// class MyApp extends StatelessWidget {
//   const MyApp({super.key});

//   @override
//   Widget build(BuildContext context) {
//     return MaterialApp(
//       title: 'Flutter Windows MethodChannel Demo',
//       theme: ThemeData(
//         primarySwatch: Colors.blue,
//       ),
//       home: const MyHomePage(),
//     );
//   }
// }

// class MyHomePage extends StatefulWidget {
//   const MyHomePage({super.key});

//   @override
//   State<MyHomePage> createState() => _MyHomePageState();
// }

// class _MyHomePageState extends State<MyHomePage> {
//   // 1. 定义一个 MethodChannel
//   // 这个通道名称必须是唯一的，并且要和 C++ 端完全一致
//   static const platform = MethodChannel('com.example.my_windows_app/computer_name');

//   String _computerName = 'Unknown';

//   // 2. 创建一个异步方法来调用原生代码
//   Future<void> _getComputerName() async {
//     String computerName;
//     try {
//       // 3. 使用 invokeMethod 调用，'getComputerName' 是我们和 C++ 约定的方法名
//       final String result = await platform.invokeMethod('getComputerName');
//       computerName = 'Computer Name: $result';
//     } on PlatformException catch (e) {
//       computerName = "Failed to get computer name: '${e.message}'.";
//     }

//     setState(() {
//       _computerName = computerName;
//     });
//   }

//   @override
//   Widget build(BuildContext context) {
//     return Scaffold(
//       appBar: AppBar(
//         title: const Text('Windows MethodChannel Demo'),
//       ),
//       body: Center(
//         child: Column(
//           mainAxisAlignment: MainAxisAlignment.center,
//           children: <Widget>[
//             Text(
//               _computerName,
//               style: const TextStyle(fontSize: 18),
//             ),
//             const SizedBox(height: 20),
//             ElevatedButton(
//               onPressed: _getComputerName,
//               child: const Text('Get Computer Name'),
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }

// import 'package:flutter/material.dart';
// import 'package:flutter/services.dart';
// import 'package:window_manager/window_manager.dart';

// void main() async {
//   WidgetsFlutterBinding.ensureInitialized();
//   await windowManager.ensureInitialized();

//   WindowOptions windowOptions = WindowOptions(
//     size: Size(800, 600),
//     center: true,
//     backgroundColor: Colors.transparent,
//     skipTaskbar: false,
//     titleBarStyle: TitleBarStyle.hidden,
//   );
//   windowManager.waitUntilReadyToShow(windowOptions, () async {
//     await windowManager.show();
//     await windowManager.focus();
//   });

//   runApp(const MyApp());
// }

// class MyApp extends StatelessWidget {
//   const MyApp({super.key});

//   @override
//   Widget build(BuildContext context) {
//     return MaterialApp(
//       title: 'Flutter Demo',
//       theme: ThemeData(
//         colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
//       ),
//       home: const MyHomePage(),
//     );
//   }
// }

// class MyHomePage extends StatefulWidget {
//   const MyHomePage({super.key});
//   @override
//   State<MyHomePage> createState() => _MyHomePageState();
// }

// class _MyHomePageState extends State<MyHomePage> {
//   int _counter = 0;
//   static const platform = MethodChannel('samples.flutter.dev/snamplaoyout');

//   void _incrementCounter() {
//     setState(() {
//       _counter++;
//     });
//   }

//   @override
//   Widget build(BuildContext context) {
//     return Scaffold(
//       body: Column(
//         children: [
//           const CustomTitleBar(title: 'Flutter Demo Home Page', platform: platform,
//           ),
//           Expanded(
//             child: Container(
//               color: Colors.white,
//               child: Center(
//                 child: Column(
//                   mainAxisAlignment: MainAxisAlignment.center,
//                   children: <Widget>[
//                     const Text('You have pushed the button this many times:'),
//                     Text(
//                       '$_counter',
//                       style: Theme.of(context).textTheme.headlineMedium,
//                     ),
//                   ],
//                 ),
//               ),
//             ),
//           ),
//         ],
//       ),
//       floatingActionButton: FloatingActionButton(
//         onPressed: _incrementCounter,
//         tooltip: 'Increment',
//         child: const Icon(Icons.add),
//       ),
//     );
//   }
// }

// class CustomTitleBar extends StatelessWidget {
//   final String title;
//   final Color backgroundColor;
//   final Color buttonColor;
//   final MethodChannel? platform;

//   const CustomTitleBar({
//     super.key,
//     required this.title,
//     this.backgroundColor = const Color(0xFFEBEBEB),
//     this.buttonColor = Colors.black,
//     this.platform,
//   });

//   @override
//   Widget build(BuildContext context) {
//     return GestureDetector(
//       onPanUpdate: (details) {
//         windowManager.startDragging();
//       },
//       child: Container(
//         height: 32,
//         color: backgroundColor,
//         child: Row(
//           children: [
//             const Spacer(),
//             TitleBarButton(
//               icon: Icons.remove,
//               onPressed: () => windowManager.minimize(),
//               color: buttonColor,
//             ),
//             FutureBuilder<bool>(
//               future: windowManager.isMaximized(),
//               builder: (context, snapshot) {
//                 var isMaximized = snapshot.data ?? false;
//                 return TitleBarButton(
//                   icon: isMaximized ? Icons.crop_square : Icons.aspect_ratio,
//                   onPressed: () {
//                     if (isMaximized) {
//                       windowManager.restore();
//                       isMaximized = !isMaximized;
//                     } else {
//                       windowManager.maximize();
//                       isMaximized = !isMaximized;
//                     }
//                   },
//                   color: buttonColor,
//                   isMaximizeButton: true,
//                 );
//               },
//             ),
//             TitleBarButton(
//               icon: Icons.close,
//               onPressed: () => windowManager.close(),
//               color: buttonColor,
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }

// class TitleBarButton extends StatelessWidget {
//   final IconData icon;
//   final VoidCallback onPressed;
//   final Color color;
//   final bool isMaximizeButton;
//   final MethodChannel? platform;

//   const TitleBarButton({
//     super.key,
//     required this.icon,
//     required this.onPressed,
//     this.color = Colors.black,
//     this.isMaximizeButton = false,
//     this.platform,
//   });

//   @override
//   Widget build(BuildContext context) {
//     return MouseRegion(
//       cursor: SystemMouseCursors.click,
//       onHover: (event) {
//         // 仅对最大化按钮触发 Snap Layouts
//         if (isMaximizeButton) {
//           StartsnapLayout();
//         }
//       },
//       child: GestureDetector(
//         onTap: onPressed,
//         child: Container(
//           width: 46,
//           height: 32,
//           margin: const EdgeInsets.only(right: 2),
//           color: Colors.transparent,
//           child: Center(child: Icon(icon, size: 16, color: color)),
//         ),
//       ),
//     );
//   }

//   Future<void> StartsnapLayout() async {
//     String batteryLevel;
//     try {
//       final result = await platform?.invokeMethod<int>('StartsnapLayout');
//     } on PlatformException catch (e) {}
//   }
// }
