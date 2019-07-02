using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsappViewer.DataSources
{
    interface IDataSource: IDisposable
    {

         IEnumerable<IMessageItem> getMessages(string ChatName);

         IEnumerable<ChatItem> getChats();

         string searchText { get;  set; }

        string connectedDbFile { get;  }

    }
}
