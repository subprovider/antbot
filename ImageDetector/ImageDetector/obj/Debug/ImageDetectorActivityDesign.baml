   M S B A M L   `   `   `  ����e  aSystem.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35wTclr-namespace:System.Activities.Presentation;assembly=System.Activities.PresentationSystem.Activities.Presentation  5      �_clr-namespace:System.Activities.Presentation.Converters;assembly=System.Activities.Presentation)System.Activities.Presentation.Converters  5      �Yclr-namespace:System.Activities.Presentation.View;assembly=System.Activities.Presentation#System.Activities.Presentation.View  5       ImageDetectorActivity5   /ktds.Ant.Activities.ImageDetectorActivityDesign   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      dsapTclr-namespace:System.Activities.Presentation;assembly=System.Activities.Presentation     5      psapc_clr-namespace:System.Activities.Presentation.Converters;assembly=System.Activities.Presentation     5      jsapvYclr-namespace:System.Activities.Presentation.View;assembly=System.Activities.Presentation     5      5   /System.Activities.Presentation.ActivityDesigner    Icon  5	      L� 5
   
    L� Drawing 5      �� 5       �� Rect 5      � 5       � Location$	 0,0&�6    � Size$ 16,16��6.   6>    �� ImageSource 5      �� 5       �� 	UriSource$ 
camera.png?�6&   6@   5      5       � 	Resources 5         	Collapsed � ColumnDefinitions	 � RowDefinitions
 �� Orientation ��Name ��Name �� Path �� Mode ��Name ��Name ��Name �� Left
 �� Top  Expanded  ExpandOrCollapsedStyle �� 
TargetType �� Property �� Value �� Triggers �� Binding �� Value��%
  &	        &	 	    &	 C	    �� 5   
   .R�5      �  5      �� 5       $��* �6'   	 5#      �� 5$      $��25 �6$   �� 5%      $��* �6$   .+�5(      �� $��0��6   $��0��6*   $
 
Horizontal=�6;   .+�5)      �  5*      �� 5+      $��30 �6/   �� 5,      $��* �6/   �� 5-      $��25 �6/   	 5/      �� 50      $��25 �6,   .+�53      �� $��0��6%   $��0��65   $
 
Horizontal=�6B   .+�54      �� $ 
btnCapture��6$   -   5        $��25��54   P   $��25��6[   $
��Left=�6g   $	��Top=�6�   .��55   !   �� $��
camera.png��6'   $
��Fill=�6;   $��18��6J   $��18��6U   56      57      �� 59      $��1��6%   $��0��65   $��Center=�6B   $
 
Horizontal=�6_   .+�5:      �� $�� ��6$   �� 5;      $��Action��6$   $��Center=�65   $	��Top=�6R   �� 5<      $ 
ActionType��6&   $��90��6r   $��Right=�6}   $��Center=�6�   ��6:   �� $ ModelItem.ActionType�$ TwoWay=�6�   �� 5=      $�� ��6$   �� 5>      $��X��6$   $��Center=�60   $	��Top=�6M   �� 5?      $ OffsetX��6&   $��40��6m   $��Right=�6x   $��Center=�6�   ��68   �� $ ModelItem.OffsetX�$ TwoWay=�6�   �� 5@      $�� ��6$   �� 5A      $��Y��6$   $��Center=�60   $	��Top=�6M   �� 5B      $ OffsetY��6&   $��40��6o   $��Right=�6z   $��Center=�6�   ��69   �� $ ModelItem.OffsetY�$ TwoWay=�6�   �� 5C      $�� ��6$   5D      �� 5E      $��2��6%   $��0��65   $��Right=�6B   $��Center=�6^   $
 
Horizontal=�6z   .+�5F      �� $ 	btnDetail��6$   -   5        $��Right=�5F   O   $	��Top=�6k   $��20��6�   .��5G       �� $��
detail.png��6&   $
��Fill=�6:   5H      5I      5J      5K      �� 5M      $��1��6   $��0��6*   $
 
Horizontal=�6:   $��Center=�6S   $��5 5 5 5 q�6p   .+�5N      �� $��1q�6   
��� ���._�5O      �� $ targetImage��6    $��Center=�65   $��5 5 5 5q�6�   ��6R   �� $ ModelItem.ImageFileName�$ TwoWay=�6�   5P      �� 5Q      $ 
CanvasMain��6!   
������ .+�5R      �� $ RecodBorderSizeObject��6!   
���  ��$��0q�6R   $ 10��6f   $ 30��6w   $��20��6�   $��20��6�   ._�5S      �� $��Center=�6(   $��Center=�6E   $��20(�6`   +6s   5T      5U      5V      5X      5Y      �� 5[   
   .R�5\      � 5^      5_      �� 5a   
   " ��6/   .�5b      ��  � ��# �   60    5c      �� 5d       6   �� $ ShowExpanded� true6D   .a�5e      ��  � ��# �  68   5f      5h      5j      .��5l      � .+�5m   
   �� #���  6   ��6L   �� 5n      5r      