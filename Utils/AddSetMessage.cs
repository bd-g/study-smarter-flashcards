using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudySmarterFlashcards.Sets;

namespace StudySmarterFlashcards.Utils
{
  public class AddSetMessage
  {
    public AddSetMessage(CardSetModel newCardSetModel)
    {
      NewCardSetModel = newCardSetModel;
    }

    public CardSetModel NewCardSetModel { get; private set; }
  }
}
