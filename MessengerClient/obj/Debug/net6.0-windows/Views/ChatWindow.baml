   M S B A M L   `   `   `  ����J  FMessengerClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:MessengerClient.ViewsMessengerClient.Views  5   	   G(clr-namespace:MessengerClient.ConvertersMessengerClient.Converters  5   	   &     MessengerClient.Views.ChatWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   5local#clr-namespace:MessengerClient.Views     5   	   ?
converters(clr-namespace:MessengerClient.Converters     5   	     � Title$
  Chat��5	   	   $	��700��6   $	��500��6#    � WindowStartupLocation$ CenterScreen=�5
   	    � 
ResizeMode$ NoResize=�5   	    � 	Resources 5      8   2MessengerClient.Converters.NicknameToTextAlignment   NicknameToTextAlignment>   8MessengerClient.Converters.NicknameToHorizontalAlignment ! NicknameToHorizontalAlignment��%:   &	        &	        5   
   6-     5   
   63   5      .��5      � $��20q�6    � RowDefinitions 5   
   �� 5      $��0.75* �6   �� 5      $��0.20* �6   �� 5      $��0.05* �6   .+�5   
   �� -   5         ��Name$ _messagesListView��5   :   $��0��6   $��False��6U   ��5      �� 5      .R�5      �� $	��410��6!   .+�5      �� $
��Bold"�6�    �� TextAlignment 6$   ��  �� Path$ SenderNickname� �� 	Converter# [  ��6�   �� *SenderNickname+6�   �� 5      $
��Wrap=�6�   $ Justify=�6�   ��6$   �� $ SenderNickname�# [ ��6�   �� *Text+5      5       ��5"      �� 5#      	 �� 
TargetType"	 ��6   .�5$      �� 
 �� Property
 � |� �� Value$ false��62    �� Triggers 5%      P� 5&       P� Property �@��
IsSelected P� Value$
 True��68   . �5'      �� 
 � �� 6;   Z� �� 5(      
 � �� 6<   Z� 5)      5+      5-      �� 5/   
   -   5         ��Name$ _messageTextBox��5/      $��1��6+   $��	0 10 0 10q�68    �� AcceptsReturn$
 True��51        HintTextBox#��[ 52      ��50      �� *MessageInputText+ �� UpdateSourceTrigger$ PropertyChanged=�.��52   7    53        	HintBrush j� 
AlignmentX j� 
AlignmentY j� Stretch !� Visual��%�   &	       !� 54      $
 Left=�60   $	 Top=�6B   $
 None=�6S    55      �� 56      $��Write message...��6    
�������6;   58      59      .��6!   5:      �� 5<   
   $��2��6   $��0 1 0 1q�6"   .+�5=      �� $��Center=�6   $��Right=�60   $	��100��5>      $
��Send��5?      {�5@      �� *IsSendMessageAllowed+��5A      �� *SendMessageCommand+5B      5C      5E      5F      