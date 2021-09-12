using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    public class ComponentNotFound : Exception { }
    public class ComponentAlreadyExist : Exception { }
    public class MaxComponentLimitExceeded : Exception { }
    public class ComponentUnregistered : Exception { }
    public class EntityNotFound : Exception { }
    
}
